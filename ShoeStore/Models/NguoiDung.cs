using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoeStore.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public string MaNguoiDung { get; set; } // Dạng VARCHAR(20) (Ví dụ: 'ND001')

        public string? AnhDaiDien { get; set; }

        public string HoTen { get; set; }

        public string Email { get; set; }

        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        public string? DiaChi { get; set; }

        public string MatKhau { get; set; }
    }
}