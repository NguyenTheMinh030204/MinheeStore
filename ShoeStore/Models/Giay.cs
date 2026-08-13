using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("Giay")] // Ánh xạ đúng với bảng Giay trong SQL Server
    public class Giay
    {
        [Key]
        public string MaGiay { get; set; } = null!;       // Khớp với MaGiay VARCHAR(30)

        public string TenGiay { get; set; } = null!;      // Khớp với TenGiay

        public decimal GiaBan { get; set; }              // Khớp với GiaBan

        public decimal? GiaCu { get; set; }             // Khớp với GiaCu (có thể null)

        public string AnhChinh { get; set; } = null!;     // Khớp với AnhChinh

        public string? MoTa { get; set; }               // Khớp với MoTa (có thể null)

        public string? KhuyenMaiUuDai { get; set; }     // MỚI THÊM: Khớp với KhuyenMaiUuDai NVARCHAR(500)

        public string MaDanhMuc { get; set; } = null!;   // Khớp với MaDanhMuc VARCHAR(20)

        public bool LaHotSale { get; set; }             // Khớp với LaHotSale BIT

        // =========================================================================
        // CÁC QUAN HỆ NAVIGATION PROPERTY (TÊN ĐÃ ĐƯỢC CHUẨN HÓA VỚI VIEW & CONTROLLER)
        // =========================================================================

        // Khóa ngoại nối sang bảng DanhMuc
        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        // Danh sách Ảnh phụ từ bảng AnhGiay
        public virtual ICollection<AnhGiay>? AnhGiay { get; set; }

        // Danh sách Biến thể (Size/Tồn kho) từ bảng BienTheGiay
        public virtual ICollection<BienTheGiay>? BienTheGiay { get; set; }
    }
}