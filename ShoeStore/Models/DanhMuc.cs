using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public string MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
    }
}