using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public string MaDonHang { get; set; } // Dạng VARCHAR(20) (Ví dụ: 'DH001')

        public string MaNguoiDung { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } = "Đang xử lý";

        public string DiaChiGiaoHang { get; set; }

        public string SoDienThoaiNhan { get; set; }

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual ICollection<ChiTietDonHang>? DanhSachChiTiet { get; set; }

        [NotMapped]
        public virtual ICollection<ChiTietDonHang>? ChiTietDonHang => DanhSachChiTiet;
    }
}