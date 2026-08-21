using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("ChiTietGioHang")]
    public class ChiTietGioHang
    {
        [Key]
        public int MaChiTietGioHang { get; set; } 

        public string MaGioHang { get; set; }

        public int MaBienThe { get; set; }

        public int SoLuong { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang? GioHang { get; set; }

        [ForeignKey("MaBienThe")]
        public virtual BienTheGiay? BienTheGiay { get; set; }
    }
}
