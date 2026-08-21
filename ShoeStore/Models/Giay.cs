using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("Giay")] 
    public class Giay
    {
        [Key]
        public string MaGiay { get; set; } = null!;       

        public string TenGiay { get; set; } = null!;      

        public decimal GiaBan { get; set; }              

        public decimal? GiaCu { get; set; }             

        public string AnhChinh { get; set; } = null!;     

        public string? MoTa { get; set; }               

        public string? KhuyenMaiUuDai { get; set; }     

        public string MaDanhMuc { get; set; } = null!;   

        public bool LaHotSale { get; set; }             

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        public virtual ICollection<AnhGiay>? AnhGiay { get; set; }

        public virtual ICollection<BienTheGiay>? BienTheGiay { get; set; }
    }
}
