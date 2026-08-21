using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string LayMaNguoiDungHienTai()
        {
            return User.FindFirst("MaNguoiDung")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? HttpContext.Session.GetString("MaNguoiDung");
        }

        [HttpGet]
        [Route("GioHang")]
        [Route("GioHang/GioHang")]
        [Route("GioHang/Index")]
        public async Task<IActionResult> Index()
        {
            string maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/GioHang" });
            }

            var gioHang = await _context.GioHang
                .Include(g => g.ChiTietGioHang)
                    .ThenInclude(c => c.BienTheGiay)
                        .ThenInclude(b => b.Giay)
                .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

            return View("GioHang", gioHang);
        }

        [HttpGet]
        public async Task<IActionResult> LayThongTinNhanh(string maGiay)
        {
            if (string.IsNullOrWhiteSpace(maGiay))
            {
                return Json(new { success = false, message = "Mã giày không hợp lệ." });
            }

            var giay = await _context.Giay
                .Include(g => g.BienTheGiay)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGiay == maGiay);

            if (giay == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin sản phẩm." });
            }

            var danhSachSize = new List<object>();
            for (int sz = 33; sz <= 41; sz++)
            {
                var bienThe = giay.BienTheGiay?.FirstOrDefault(b => b.KichCo == sz);
                int tonKho = bienThe != null ? bienThe.SoLuongTon : 0;
                int maBienThe = bienThe != null ? bienThe.MaBienThe : 0;
                bool conHang = bienThe != null && tonKho > 0;

                danhSachSize.Add(new
                {
                    size = sz,
                    maBienThe = maBienThe,
                    tonKho = tonKho,
                    conHang = conHang
                });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    maGiay = giay.MaGiay,
                    tenGiay = giay.TenGiay,
                    anhChinh = giay.AnhChinh,
                    giaBan = giay.GiaBan,
                    sizes = danhSachSize
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> ThemVaoGioHang(int maBienThe, int soLuong)
        {
            string maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!",
                    redirectUrl = "/TaiKhoan/DangNhap"
                });
            }

            if (maBienThe <= 0 || soLuong <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ." });
            }

            var bienThe = await _context.BienTheGiay.FindAsync(maBienThe);
            if (bienThe == null)
            {
                return Json(new { success = false, message = "Biến thể giày không tồn tại trong hệ thống." });
            }

            if (bienThe.SoLuongTon < soLuong)
            {
                return Json(new { success = false, message = $"Số lượng tồn kho không đủ (Hiện còn: {bienThe.SoLuongTon})." });
            }

            var gioHang = await _context.GioHang
                .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

            if (gioHang == null)
            {
                int countGioHang = await _context.GioHang.CountAsync() + 1;
                string newMaGH = $"GH{countGioHang:D3}";

                while (await _context.GioHang.AnyAsync(g => g.MaGioHang == newMaGH))
                {
                    countGioHang++;
                    newMaGH = $"GH{countGioHang:D3}";
                }

                gioHang = new GioHang
                {
                    MaGioHang = newMaGH,
                    MaNguoiDung = maNguoiDung,
                    NgayTao = DateTime.Now
                };

                _context.GioHang.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            var chiTiet = await _context.ChiTietGioHang
                .FirstOrDefaultAsync(c => c.MaGioHang == gioHang.MaGioHang && c.MaBienThe == maBienThe);

            if (chiTiet != null)
            {
                int tongSoLuongMoi = chiTiet.SoLuong + soLuong;
                if (tongSoLuongMoi > bienThe.SoLuongTon)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Số lượng vượt quá tồn kho! Trong giỏ đã có {chiTiet.SoLuong} đôi."
                    });
                }
                chiTiet.SoLuong = tongSoLuongMoi;
            }
            else
            {
                chiTiet = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaBienThe = maBienThe,
                    SoLuong = soLuong
                };
                _context.ChiTietGioHang.Add(chiTiet);
            }

            await _context.SaveChangesAsync();

            int tongMon = await _context.ChiTietGioHang
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .SumAsync(c => (int?)c.SoLuong) ?? 0;

            return Json(new
            {
                success = true,
                message = "Thêm vào giỏ hàng thành công!",
                totalItems = tongMon
            });
        }

        [HttpGet]
        public async Task<IActionResult> LayTongSoLuongGioHang()
        {
            string maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Json(new { totalItems = 0 });
            }

            var gioHang = await _context.GioHang
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

            if (gioHang == null)
            {
                return Json(new { totalItems = 0 });
            }

            int tongMon = await _context.ChiTietGioHang
                .Where(c => c.MaGioHang == gioHang.MaGioHang)
                .SumAsync(c => (int?)c.SoLuong) ?? 0;

            return Json(new { totalItems = tongMon });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatSoLuong(int maChiTiet, int soLuong)
        {
            if (maChiTiet <= 0 || soLuong <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            var chiTiet = await _context.ChiTietGioHang
                .Include(c => c.BienTheGiay)
                    .ThenInclude(b => b.Giay)
                .FirstOrDefaultAsync(c => c.MaChiTietGioHang == maChiTiet);

            if (chiTiet == null)
            {
                return Json(new { success = false, message = "Mục giỏ hàng không tồn tại." });
            }

            if (chiTiet.BienTheGiay.SoLuongTon < soLuong)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trong kho chỉ còn tối đa {chiTiet.BienTheGiay.SoLuongTon} đôi!",
                    maxStock = chiTiet.BienTheGiay.SoLuongTon
                });
            }

            chiTiet.SoLuong = soLuong;
            await _context.SaveChangesAsync();

            var dsChiTiet = await _context.ChiTietGioHang
                .Where(c => c.MaGioHang == chiTiet.MaGioHang)
                .Include(c => c.BienTheGiay)
                    .ThenInclude(b => b.Giay)
                .ToListAsync();

            decimal tongTienGioHang = dsChiTiet.Sum(c => c.SoLuong * c.BienTheGiay.Giay.GiaBan);
            decimal thanhTienMon = chiTiet.SoLuong * chiTiet.BienTheGiay.Giay.GiaBan;
            int tongSoLuong = dsChiTiet.Sum(c => c.SoLuong);

            return Json(new
            {
                success = true,
                thanhTienMon = thanhTienMon.ToString("N0") + " VNĐ",
                tongTien = tongTienGioHang.ToString("N0") + " VNĐ",
                totalItems = tongSoLuong
            });
        }

        [HttpPost]
        public async Task<IActionResult> XoaKhoiGioHang(int maChiTiet)
        {
            var chiTiet = await _context.ChiTietGioHang.FindAsync(maChiTiet);
            if (chiTiet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy món hàng cần xóa." });
            }

            string maGioHang = chiTiet.MaGioHang;
            _context.ChiTietGioHang.Remove(chiTiet);
            await _context.SaveChangesAsync();

            var dsConLai = await _context.ChiTietGioHang
                .Where(c => c.MaGioHang == maGioHang)
                .Include(c => c.BienTheGiay)
                    .ThenInclude(b => b.Giay)
                .ToListAsync();

            decimal tongTien = dsConLai.Sum(c => c.SoLuong * c.BienTheGiay.Giay.GiaBan);
            int tongSoLuong = dsConLai.Sum(c => c.SoLuong);

            return Json(new
            {
                success = true,
                message = "Đã xóa sản phẩm khỏi giỏ hàng.",
                tongTien = tongTien.ToString("N0") + " VNĐ",
                totalItems = tongSoLuong,
                isEmpty = !dsConLai.Any()
            });
        }
    }
}
