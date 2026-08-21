using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("AnhGiay")]
    public class AnhGiay
    {
        [Key]
        public int MaAnh { get; set; }           

        public string MaGiay { get; set; }       

        public string DuongDanAnh { get; set; }   

        [ForeignKey("MaGiay")]
        public virtual Giay? Giay { get; set; }
    }
}
