using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public string MaGioHang { get; set; } // Dạng VARCHAR(20) (Ví dụ: 'GH001')

        public string MaNguoiDung { get; set; } // Khóa ngoại trỏ đến NguoiDung

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual ICollection<ChiTietGioHang>? DanhSachChiTiet { get; set; }
    }
}