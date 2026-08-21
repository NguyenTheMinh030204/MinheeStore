using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("BienTheGiay")]
    public class BienTheGiay
    {
        [Key]
        public int MaBienThe { get; set; }        

        public string MaGiay { get; set; }        

        public int KichCo { get; set; }           

        public int SoLuongTon { get; set; }       

        [ForeignKey("MaGiay")]
        public virtual Giay? Giay { get; set; }
    }
}
