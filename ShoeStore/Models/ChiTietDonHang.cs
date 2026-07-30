using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("ChiTietDonHang")]
    public class ChiTietDonHang
    {
        [Key]
        public int MaChiTiet { get; set; } // Mã tự tăng (IDENTITY)

        public string MaDonHang { get; set; }

        public int MaBienThe { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("MaBienThe")]
        public virtual BienTheGiay? BienTheGiay { get; set; }
    }
}