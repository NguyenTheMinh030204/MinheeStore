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

        [Authorize]
        [HttpGet]
        public IActionResult ThongTinCaNhan()
        {
            
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            return View(user); 
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTin(NguoiDung model, IFormFile? AnhDaiDienMoi)
        {
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return RedirectToAction("DangNhap");
            }

            user.HoTen = model.HoTen;
            user.SoDienThoai = model.SoDienThoai;
            user.NgaySinh = model.NgaySinh;
            user.DiaChi = model.DiaChi;

            if (AnhDaiDienMoi != null && AnhDaiDienMoi.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(AnhDaiDienMoi.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhDaiDienMoi.CopyToAsync(fileStream);
                }

                user.AnhDaiDien = "/images/avatars/" + uniqueFileName;
            }

            _db.NguoiDung.Update(user);
            await _db.SaveChangesAsync();

            TempData["ThongBaoSuccess"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            
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
            
            ViewBag.Email = Email;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == Email && u.MatKhau == MatKhau);
            if (user != null)
            {
                
                string token = GenerateJwtToken(user);

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

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

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
                
                string otpCode = Random.Shared.Next(100000, 999999).ToString();

                string cacheKey = $"OTP_{email.Trim().ToLower()}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

                await _emailService.SendOtpEmailAsync(email, otpCode);

                return Json(new { success = true, message = "Mã OTP đã được gửi về Email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi Email: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult KiemTraOTP([FromQuery] string email, [FromQuery] string otpCode)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            string cleanEmail = email.Trim().ToLower();
            string keyReg = $"OTP_{cleanEmail}";
            string keyReset = $"OTP_RESET_{cleanEmail}";

            if ((_cache.TryGetValue(keyReg, out string? validOtpReg) && validOtpReg == otpCode) ||
                (_cache.TryGetValue(keyReset, out string? validOtpReset) && validOtpReset == otpCode))
            {
                return Json(new { success = true, message = "Mã OTP hợp lệ!" });
            }

            return Json(new { success = false, message = "Mã OTP không chính xác hoặc đã hết hạn!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(NguoiDung model, string XacNhanMatKhau, string OtpCode, IFormFile? AnhDaiDien)
        {
            ViewBag.XacNhanMatKhau = XacNhanMatKhau;
            ViewBag.OtpCode = OtpCode;

            if (model.MatKhau != XacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View(model);
            }

            string cacheKey = $"OTP_{model.Email?.Trim().ToLower()}";
            if (!_cache.TryGetValue(cacheKey, out string? validOtp) || validOtp != OtpCode)
            {
                ViewBag.Loi = "Mã OTP không chính xác hoặc đã hết hạn!";
                return View(model);
            }

            bool isExist = _db.NguoiDung.Any(u => u.Email == model.Email);
            if (isExist)
            {
                ViewBag.Loi = "Email này đã được sử dụng để đăng ký!";
                return View(model);
            }

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

            _db.NguoiDung.Add(model);
            await _db.SaveChangesAsync();

            _cache.Remove(cacheKey);

            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public IActionResult DangXuat()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuiOTPQuenMatKhau([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { success = false, message = "Email không được để trống!" });
            }

            bool isExist = _db.NguoiDung.Any(u => u.Email == email);
            if (!isExist)
            {
                return Json(new { success = false, message = "Email này chưa được đăng ký tài khoản!" });
            }

            try
            {
                string otpCode = Random.Shared.Next(100000, 999999).ToString();
                string cacheKey = $"OTP_RESET_{email.Trim().ToLower()}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

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
        public async Task<IActionResult> QuenMatKhau(string Email, string OtpCode, string MatKhauMoi, string XacNhanMatKhau)
        {
            ViewBag.Email = Email;
            ViewBag.OtpCode = OtpCode;

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            string cacheKey = $"OTP_RESET_{Email?.Trim().ToLower()}";
            if (!_cache.TryGetValue(cacheKey, out string? validOtp) || validOtp != OtpCode)
            {
                ViewBag.Loi = "Mã OTP không chính xác hoặc đã hết hạn!";
                return View();
            }

            var user = _db.NguoiDung.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.Loi = "Tài khoản không tồn tại!";
                return View();
            }

            user.MatKhau = MatKhauMoi;
            _db.NguoiDung.Update(user);
            await _db.SaveChangesAsync();

            _cache.Remove(cacheKey);

            TempData["ThongBaoSuccess"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction("DangNhap");
        }

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
