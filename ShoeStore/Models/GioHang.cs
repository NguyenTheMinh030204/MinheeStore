using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public string MaGioHang { get; set; } = null!;

        public string MaNguoiDung { get; set; } = null!;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual ICollection<ChiTietGioHang> ChiTietGioHang { get; set; } = new List<ChiTietGioHang>();
    }
}
