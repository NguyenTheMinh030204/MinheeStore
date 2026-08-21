using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public string MaDonHang { get; set; } = null!;
        public string MaNguoiDung { get; set; } = null!;
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "Đang xử lý";
        public string DiaChiGiaoHang { get; set; } = null!;
        public string SoDienThoaiNhan { get; set; } = null!;

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }
        public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; } = new List<ChiTietDonHang>();
    }
}
