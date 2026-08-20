using System.Collections.Generic;

namespace ShoeStore.Models.ViewModels
{
    public class ItemThanhToanVM
    {
        public int MaBienThe { get; set; }
        public string TenGiay { get; set; } = "";
        public string AnhChinh { get; set; } = "";
        public int KichCo { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string? KhuyenMaiUuDai { get; set; } // Lấy từ bảng Giay
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class ThanhToanVM
    {
        public string HoTenNguoiNhan { get; set; } = "";
        public string EmailNguoiNhan { get; set; } = "";
        public string SoDienThoaiNhan { get; set; } = "";
        public string DiaChiGiaoHang { get; set; } = "";

        public List<ItemThanhToanVM> DanhSachItem { get; set; } = new();

        public decimal TongTienHang { get; set; }
        public decimal PhiVanChuyen { get; set; } = 30000;
        public decimal TongTienThanhToan => TongTienHang + PhiVanChuyen;

        public bool IsMuaNgay { get; set; }
    }

    public class DatHangRequest
    {
        public string HoTenNguoiNhan { get; set; } = "";
        public string EmailNguoiNhan { get; set; } = "";
        public string SoDienThoaiNhan { get; set; } = "";
        public string DiaChiGiaoHang { get; set; } = "";
        public string PhuongThucThanhToan { get; set; } = "COD";
        public decimal PhiVanChuyen { get; set; } = 30000;

        public List<int>? DanhSachMaChiTietChon { get; set; }
        public int? MaBienTheMuaNgay { get; set; }
        public int? SoLuongMuaNgay { get; set; }
    }
}