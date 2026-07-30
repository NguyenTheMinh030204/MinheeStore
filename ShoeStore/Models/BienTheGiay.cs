using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("BienTheGiay")]
    public class BienTheGiay
    {
        [Key]
        public int MaBienThe { get; set; }        // Mã biến thể tự tăng (IDENTITY)

        public string MaGiay { get; set; }        // Khớp với MaGiay dạng VARCHAR(30) (Ví dụ: 'G03022004-001')

        public string MauSac { get; set; }        // Màu sắc sản phẩm (Ví dụ: N'Đỏ', N'Xanh')

        public int KichCo { get; set; }           // Size giày (Ví dụ: 28, 29, 30)

        public int SoLuongTon { get; set; }       // Số lượng hàng còn trong kho

        // Khóa ngoại liên kết ngược lại bảng Giay
        [ForeignKey("MaGiay")]
        public virtual Giay? Giay { get; set; }
    }
}