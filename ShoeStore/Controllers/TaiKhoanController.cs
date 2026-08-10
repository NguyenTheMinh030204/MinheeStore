using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Data;
using ShoeStore.Models;
using ShoeStore.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;

        public TaiKhoanController(
            ApplicationDbContext db,
            IConfiguration config,
            IWebHostEnvironment env,
            EmailService emailService,
            IMemoryCache cache)
        {
            _db = db;
            _config = config;
            _env = env;
            _emailService = emailService;
            _cache = cache;
        }

        // ==========================================
        // 1. TRANG THÔNG TIN CÁ NHÂN (PROFILE)
        // ==========================================
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            // Lấy Email người dùng đã lưu trong JWT Claims
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            return View(user);
        }

        // ==========================================
        // 2. ĐĂNG NHẬP (DANG NHAP)
        // ==========================================
        [HttpGet]
        public IActionResult DangNhap()
        {
            // Nếu người dùng đã đăng nhập rồi thì chuyển thẳng về Trang chủ
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("TrangChu", "TrangChu");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangNhap(string Email, string MatKhau)
        {
            // Giữ lại Email để hiển thị lại trên View nếu có lỗi
            ViewBag.Email = Email;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            // Kiểm tra thông tin tài khoản trong CSDL SQL Server
            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == Email && u.MatKhau == MatKhau);
            if (user != null)
            {
                // 1. Sinh JWT Token bằng Secret Key
                string token = GenerateJwtToken(user);

                // 2. Lưu JWT Token vào HttpOnly Cookie an toàn
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

                return RedirectToAction("TrangChu", "TrangChu");
            }

            ViewBag.Loi = "Email hoặc Mật khẩu không chính xác!";
            return View();
        }

        // ==========================================
        // 3. ĐĂNG KÝ (DANG KY) & AJAX GỬI OTP THẬT
        // ==========================================
        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        // API AJAX GỬI OTP VỀ EMAIL THẬT
        [HttpPost]
        public async Task<IActionResult> GuiOTP([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Email không được để trống!" });
            }

            bool isExist = _db.NguoiDung.Any(u => u.Email == email);
            if (isExist)
            {
                return Json(new { success = false, message = "Email này đã được sử dụng!" });
            }

            try
            {
                // 1. Sinh ngẫu nhiên mã OTP 6 chữ số (ví dụ: 683920)
                string otpCode = Random.Shared.Next(100000, 999999).ToString();

                // 2. Lưu OTP vào IMemoryCache trong vòng 5 phút (Key = OTP_email)
                string cacheKey = $"OTP_{email.Trim().ToLower()}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

                // 3. Gửi Email thật bằng SmtpClient
                await _emailService.SendOtpEmailAsync(email, otpCode);

                return Json(new { success = true, message = "Mã OTP đã được gửi về Email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi Email: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(NguoiDung model, string XacNhanMatKhau, string OtpCode, IFormFile? AnhDaiDien)
        {
            // Truyền các tham số ngoài Model vào ViewBag để giữ lại khi bị reload do lỗi
            ViewBag.XacNhanMatKhau = XacNhanMatKhau;
            ViewBag.OtpCode = OtpCode;

            // 1. Kiểm tra mật khẩu xác nhận
            if (model.MatKhau != XacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View(model);
            }

            // 2. Xác thực mã OTP từ Memory Cache
            string cacheKey = $"OTP_{model.Email?.Trim().ToLower()}";
            if (!_cache.TryGetValue(cacheKey, out string? validOtp) || validOtp != OtpCode)
            {
                ViewBag.Loi = "Mã OTP không chính xác hoặc đã hết hạn!";
                return View(model);
            }

            // 3. Kiểm tra Email đã tồn tại chưa
            bool isExist = _db.NguoiDung.Any(u => u.Email == model.Email);
            if (isExist)
            {
                ViewBag.Loi = "Email này đã được sử dụng để đăng ký!";
                return View(model);
            }

            // --------------------------------------------------
            // A. TỰ ĐỘNG SINH MÃ NGƯỜI DÙNG: ND03022004_xxx
            // --------------------------------------------------
            string prefix = "ND03022004_";

            var maxUserCode = _db.NguoiDung
                .Where(u => u.MaNguoiDung.StartsWith(prefix))
                .Select(u => u.MaNguoiDung)
                .OrderByDescending(code => code)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(maxUserCode) && maxUserCode.Length >= prefix.Length + 3)
            {
                string suffix = maxUserCode.Substring(prefix.Length);
                if (int.TryParse(suffix, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            model.MaNguoiDung = $"{prefix}{nextNumber:D3}";

            // --------------------------------------------------
            // B. GÁN VAI TRÒ MẶC ĐỊNH & XỬ LÝ UPLOAD ANH AVATAR
            // --------------------------------------------------
            model.VaiTro = "Khách hàng";

            if (AnhDaiDien != null && AnhDaiDien.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(AnhDaiDien.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhDaiDien.CopyToAsync(fileStream);
                }

                model.AnhDaiDien = "/images/avatars/" + uniqueFileName;
            }

            // --------------------------------------------------
            // C. LƯU VÀO CSDL VÀ XÓA OTP CACHE
            // --------------------------------------------------
            _db.NguoiDung.Add(model);
            await _db.SaveChangesAsync();

            // Xóa OTP khỏi Cache sau khi đăng ký thành công
            _cache.Remove(cacheKey);

            return RedirectToAction("DangNhap");
        }

        // ==========================================
        // 4. ĐĂNG XUẤT (LOGOUT)
        // ==========================================
        [HttpGet]
        public IActionResult DangXuat()
        {
            // Xóa Cookie chứa JWT Token
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("DangNhap");
        }

        // ==========================================
        // HÀM BỔ TRỢ: KÝ SINH JWT TOKEN
        // ==========================================
        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                jwtSettings["Key"] ?? "MinheeShop_Super_Secret_Key_2026_DotNet8_JWT_Authentication"));

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.HoTen ?? "Khách hàng"),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "Khách hàng"),
                new Claim("MaNguoiDung", user.MaNguoiDung ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpireHours"] ?? "2")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}