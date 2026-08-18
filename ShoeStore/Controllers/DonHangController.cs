using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Màn hình xem lại thông tin & nhập địa chỉ nhận hàng
        [HttpGet]
        public async Task<IActionResult> ThanhToan(int maBienThe, int soLuong = 1)
        {
            string maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = $"/DonHang/ThanhToan?maBienThe={maBienThe}&soLuong={soLuong}" });
            }

            var bienThe = await _context.BienTheGiay
                .Include(b => b.Giay)
                .FirstOrDefaultAsync(b => b.MaBienThe == maBienThe);

            if (bienThe == null || bienThe.SoLuongTon < soLuong)
            {
                TempData["Error"] = "Sản phẩm hoặc Size này hiện đã hết hàng!";
                return RedirectToAction("TrangChu", "TrangChu");
            }

            var nguoiDung = await _context.NguoiDung.FindAsync(maNguoiDung);

            ViewBag.BienThe = bienThe;
            ViewBag.SoLuong = soLuong;
            ViewBag.NguoiDung = nguoiDung;
            ViewBag.TongTien = bienThe.Giay.GiaBan * soLuong;

            return View();
        }

        // Xác nhận đặt hàng -> Lưu vào 2 bảng SQL
        [HttpPost]
        public async Task<IActionResult> XacNhanDatHang(int maBienThe, int soLuong, string diaChiGiaoHang, string soDienThoaiNhan)
        {
            string maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var bienThe = await _context.BienTheGiay
                .Include(b => b.Giay)
                .FirstOrDefaultAsync(b => b.MaBienThe == maBienThe);

            if (bienThe == null || bienThe.SoLuongTon < soLuong)
            {
                TempData["Error"] = "Số lượng trong kho không đủ đáp ứng!";
                return RedirectToAction("ThanhToan", new { maBienThe, soLuong });
            }

            int countDonHang = await _context.DonHang.CountAsync() + 1;
            string newMaDH = $"DH{countDonHang:D3}";
            while (await _context.DonHang.AnyAsync(d => d.MaDonHang == newMaDH))
            {
                countDonHang++;
                newMaDH = $"DH{countDonHang:D3}";
            }

            decimal tongTien = bienThe.Giay.GiaBan * soLuong;

            // 1. Lưu Bảng DonHang
            var donHang = new DonHang
            {
                MaDonHang = newMaDH,
                MaNguoiDung = maNguoiDung,
                NgayDat = DateTime.Now,
                TongTien = tongTien,
                TrangThai = "Đang xử lý",
                DiaChiGiaoHang = diaChiGiaoHang,
                SoDienThoaiNhan = soDienThoaiNhan
            };
            _context.DonHang.Add(donHang);

            // 2. Lưu Bảng ChiTietDonHang
            var chiTiet = new ChiTietDonHang
            {
                MaDonHang = newMaDH,
                MaBienThe = maBienThe,
                SoLuong = soLuong,
                DonGia = bienThe.Giay.GiaBan
            };
            _context.ChiTietDonHang.Add(chiTiet);

            // 3. Trừ kho
            bienThe.SoLuongTon -= soLuong;

            await _context.SaveChangesAsync();

            return RedirectToAction("DatHangThanhCong", new { maDonHang = newMaDH });
        }

        // Màn hình thông báo đặt hàng thành công
        public async Task<IActionResult> DatHangThanhCong(string maDonHang)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            return View(donHang);
        }
    }
}