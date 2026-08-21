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
using ShoeStore.Models.ViewModels;

namespace ShoeStore.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? LayMaNguoiDungHienTai()
        {
            return User.FindFirst("MaNguoiDung")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? HttpContext.Session.GetString("MaNguoiDung");
        }

        [HttpGet]
        [Route("DonHang/ThanhToan")]
        [Route("GioHang/ThanhToan")]
        public async Task<IActionResult> ThanhToan(string? ids, int? maBienThe, int? soLuong)
        {
            string? maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                string returnUrl = Request.Path + Request.QueryString;
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl });
            }

            var nguoiDung = await _context.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

            var vm = new ThanhToanVM
            {
                HoTenNguoiNhan = nguoiDung?.HoTen ?? "",
                EmailNguoiNhan = nguoiDung?.Email ?? "",
                SoDienThoaiNhan = nguoiDung?.SoDienThoai ?? "",
                DiaChiGiaoHang = nguoiDung?.DiaChi ?? "",
                PhiVanChuyen = 30000
            };

            if (maBienThe.HasValue && maBienThe > 0 && soLuong.HasValue && soLuong > 0)
            {
                var bienThe = await _context.BienTheGiay
                    .Include(b => b.Giay)
                    .FirstOrDefaultAsync(b => b.MaBienThe == maBienThe.Value);

                if (bienThe == null || bienThe.SoLuongTon < soLuong.Value)
                {
                    return RedirectToAction("TrangChu", "TrangChu");
                }

                vm.IsMuaNgay = true;
                vm.DanhSachItem.Add(new ItemThanhToanVM
                {
                    MaBienThe = bienThe.MaBienThe,
                    TenGiay = bienThe.Giay?.TenGiay ?? "",
                    AnhChinh = bienThe.Giay?.AnhChinh ?? "",
                    KichCo = bienThe.KichCo,
                    SoLuong = soLuong.Value,
                    DonGia = bienThe.Giay?.GiaBan ?? 0,
                    KhuyenMaiUuDai = bienThe.Giay?.KhuyenMaiUuDai
                });
            }
            
            else
            {
                vm.IsMuaNgay = false;

                var gioHang = await _context.GioHang
                    .Include(g => g.ChiTietGioHang)
                        .ThenInclude(c => c.BienTheGiay)
                            .ThenInclude(b => b.Giay)
                    .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

                if (gioHang == null || !gioHang.ChiTietGioHang.Any())
                {
                    return RedirectToAction("GioHang", "GioHang");
                }

                var idList = new List<int>();
                if (!string.IsNullOrWhiteSpace(ids))
                {
                    idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => int.TryParse(x.Trim(), out int val) ? val : 0)
                                .Where(x => x > 0)
                                .ToList();
                }

                var items = idList.Any()
                    ? gioHang.ChiTietGioHang.Where(c => idList.Contains(c.MaChiTietGioHang)).ToList()
                    : gioHang.ChiTietGioHang.ToList();

                if (!items.Any())
                {
                    return RedirectToAction("GioHang", "GioHang");
                }

                foreach (var item in items)
                {
                    vm.DanhSachItem.Add(new ItemThanhToanVM
                    {
                        MaBienThe = item.MaBienThe,
                        TenGiay = item.BienTheGiay?.Giay?.TenGiay ?? "",
                        AnhChinh = item.BienTheGiay?.Giay?.AnhChinh ?? "",
                        KichCo = item.BienTheGiay?.KichCo ?? 0,
                        SoLuong = item.SoLuong,
                        DonGia = item.BienTheGiay?.Giay?.GiaBan ?? 0,
                        KhuyenMaiUuDai = item.BienTheGiay?.Giay?.KhuyenMaiUuDai
                    });
                }
            }

            vm.TongTienHang = vm.DanhSachItem.Sum(i => i.ThanhTien);
            return View("~/Views/GioHang/ThanhToan.cshtml", vm);
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanDatHang([FromBody] DatHangRequest req)
        {
            string? maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });
            }

            if (string.IsNullOrWhiteSpace(req.SoDienThoaiNhan) ||
                string.IsNullOrWhiteSpace(req.DiaChiGiaoHang) ||
                string.IsNullOrWhiteSpace(req.HoTenNguoiNhan))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin giao hàng!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<(int MaBienThe, int SoLuong, decimal DonGia)> listItemsToOrder = new();

                if (req.MaBienTheMuaNgay.HasValue && req.SoLuongMuaNgay.HasValue)
                {
                    var bienThe = await _context.BienTheGiay
                        .Include(b => b.Giay)
                        .FirstOrDefaultAsync(b => b.MaBienThe == req.MaBienTheMuaNgay.Value);

                    if (bienThe == null || bienThe.SoLuongTon < req.SoLuongMuaNgay.Value)
                    {
                        return Json(new { success = false, message = "Sản phẩm không đủ tồn kho!" });
                    }
                    listItemsToOrder.Add((bienThe.MaBienThe, req.SoLuongMuaNgay.Value, bienThe.Giay?.GiaBan ?? 0));
                }
                else
                {
                    var gioHang = await _context.GioHang
                        .Include(g => g.ChiTietGioHang)
                            .ThenInclude(c => c.BienTheGiay)
                                .ThenInclude(b => b.Giay)
                        .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung);

                    if (gioHang == null || !gioHang.ChiTietGioHang.Any())
                    {
                        return Json(new { success = false, message = "Giỏ hàng rỗng." });
                    }

                    var items = req.DanhSachMaChiTietChon != null && req.DanhSachMaChiTietChon.Any()
                        ? gioHang.ChiTietGioHang.Where(c => req.DanhSachMaChiTietChon.Contains(c.MaChiTietGioHang)).ToList()
                        : gioHang.ChiTietGioHang.ToList();

                    foreach (var it in items)
                    {
                        if (it.BienTheGiay == null || it.BienTheGiay.SoLuongTon < it.SoLuong)
                        {
                            return Json(new { success = false, message = $"Sản phẩm {it.BienTheGiay?.Giay?.TenGiay} (Size {it.BienTheGiay?.KichCo}) không đủ hàng tồn kho!" });
                        }
                        listItemsToOrder.Add((it.MaBienThe, it.SoLuong, it.BienTheGiay?.Giay?.GiaBan ?? 0));
                    }

                    _context.ChiTietGioHang.RemoveRange(items);
                }

                int countDonHang = await _context.DonHang.CountAsync() + 1;
                string maDH = $"DH{countDonHang:D3}";
                while (await _context.DonHang.AnyAsync(d => d.MaDonHang == maDH))
                {
                    countDonHang++;
                    maDH = $"DH{countDonHang:D3}";
                }

                decimal tongTienHang = listItemsToOrder.Sum(i => i.SoLuong * i.DonGia);
                decimal tongTienCuoi = tongTienHang + req.PhiVanChuyen;

                var donHang = new DonHang
                {
                    MaDonHang = maDH,
                    MaNguoiDung = maNguoiDung,
                    NgayDat = DateTime.Now,
                    TongTien = tongTienCuoi,
                    TrangThai = req.PhuongThucThanhToan == "COD" ? "Đang xử lý" : "Chờ chuyển khoản",
                    DiaChiGiaoHang = req.DiaChiGiaoHang.Trim(),
                    SoDienThoaiNhan = req.SoDienThoaiNhan.Trim()
                };

                _context.DonHang.Add(donHang);
                await _context.SaveChangesAsync();

                foreach (var it in listItemsToOrder)
                {
                    var ct = new ChiTietDonHang
                    {
                        MaDonHang = maDH,
                        MaBienThe = it.MaBienThe,
                        SoLuong = it.SoLuong,
                        DonGia = it.DonGia
                    };
                    _context.ChiTietDonHang.Add(ct);

                    var bt = await _context.BienTheGiay.FindAsync(it.MaBienThe);
                    if (bt != null)
                    {
                        bt.SoLuongTon -= it.SoLuong;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                string redirectUrl = req.PhuongThucThanhToan == "BANKING"
                    ? $"/DonHang/CongThanhToanOnline?maDonHang={maDH}"
                    : $"/DonHang/DatHangThanhCong?maDonHang={maDH}";

                return Json(new
                {
                    success = true,
                    message = "Đặt hàng thành công!",
                    maDonHang = maDH,
                    redirectUrl = redirectUrl
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi tạo đơn hàng: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("DonHang/CongThanhToanOnline")]
        [Route("GioHang/CongThanhToanOnline")]
        public async Task<IActionResult> CongThanhToanOnline(string maDonHang)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(c => c.BienTheGiay)
                        .ThenInclude(b => b.Giay)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null) return RedirectToAction("TrangChu", "TrangChu");

            return View("~/Views/GioHang/CongThanhToanOnline.cshtml", donHang);
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToanOnline(string maDonHang)
        {
            var donHang = await _context.DonHang.FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);
            if (donHang == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            donHang.TrangThai = "Đã thanh toán";
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                redirectUrl = $"/DonHang/DatHangThanhCong?maDonHang={maDonHang}"
            });
        }

        [HttpGet]
        public async Task<IActionResult> DatHangThanhCong(string maDonHang)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(c => c.BienTheGiay)
                        .ThenInclude(b => b.Giay)
                .Include(d => d.NguoiDung)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null) return RedirectToAction("TrangChu", "TrangChu");

            return View("~/Views/GioHang/DatHangThanhCong.cshtml", donHang);
        }

        [HttpGet]
        [Route("DonHang/DonHangCuaToi")]
        [Route("GioHang/DonHangCuaToi")]
        public async Task<IActionResult> DonHangCuaToi()
        {
            string? maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = "/DonHang/DonHangCuaToi" });
            }

            var danhSachDonHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(c => c.BienTheGiay)
                        .ThenInclude(b => b.Giay)
                .Where(d => d.MaNguoiDung == maNguoiDung)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/GioHang/DonHangCuaToi.cshtml", danhSachDonHang);
        }

        [HttpPost]
        public async Task<IActionResult> HuyDonHang(string maDonHang)
        {
            string? maNguoiDung = LayMaNguoiDungHienTai();
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });
            }

            if (string.IsNullOrWhiteSpace(maDonHang))
            {
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = await _context.DonHang
                    .Include(d => d.ChiTietDonHang)
                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang && d.MaNguoiDung == maNguoiDung);

                if (donHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                if (donHang.TrangThai != "Đang xử lý" && donHang.TrangThai != "Chờ chuyển khoản" && donHang.TrangThai != "Chờ thanh toán online")
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng khi chưa chuyển giao!" });
                }

                foreach (var item in donHang.ChiTietDonHang)
                {
                    var bt = await _context.BienTheGiay.FindAsync(item.MaBienThe);
                    if (bt != null)
                    {
                        bt.SoLuongTon += item.SoLuong;
                    }
                }

                donHang.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi khi hủy đơn hàng: " + ex.Message });
            }
        }
    }
}
