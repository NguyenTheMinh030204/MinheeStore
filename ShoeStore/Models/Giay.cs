using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("Giay")] // Ánh xạ đúng với bảng Giay trong SQL Server
    public class Giay
    {
        [Key]
        public string MaGiay { get; set; }        // Khớp với MaGiay dạng VARCHAR(30) (Ví dụ: 'G03022004-001')

        public string TenGiay { get; set; }       // Khớp với TenGiay trong SQL

        public decimal GiaBan { get; set; }       // Khớp với GiaBan trong SQL

        public decimal? GiaCu { get; set; }      // Khớp với GiaCu trong SQL (có thể null)

        public string AnhChinh { get; set; }    // Khớp với AnhChinh hiển thị ở Trang Chủ

        public string? MoTa { get; set; }        // Mô tả chi tiết (có thể null)

        public string MaDanhMuc { get; set; }    // Khớp với MaDanhMuc: 'BeTrai', 'BeGai', 'BongRo', 'Sandal'

        public bool LaHotSale { get; set; }      // Khớp với LaHotSale trong SQL

        // Quan hệ 1 - Nhiều: Lấy 3 ảnh phụ và danh sách Size/Màu sắc
        public virtual ICollection<AnhGiay>? DanhSachAnhPhu { get; set; }
        public virtual ICollection<BienTheGiay>? DanhSachBienThe { get; set; }
    }
}