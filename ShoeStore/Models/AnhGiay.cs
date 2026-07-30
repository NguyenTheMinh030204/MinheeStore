using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("AnhGiay")]
    public class AnhGiay
    {
        [Key]
        public int MaAnh { get; set; }           // Mã ảnh tự tăng (IDENTITY)

        public string MaGiay { get; set; }       // Khớp với MaGiay dạng VARCHAR(30) (Ví dụ: 'G03022004-001')

        public string DuongDanAnh { get; set; }   // Đường dẫn đến ảnh phụ

        // Khóa ngoại liên kết ngược lại bảng Giay
        [ForeignKey("MaGiay")]
        public virtual Giay? Giay { get; set; }
    }
}