using System;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class NguoiDung
    {
        [Key] // Giữ lại thẻ này để EF Core biết MaNguoiDung là Khóa chính
        public string MaNguoiDung { get; set; } = null!;

        public string? AnhDaiDien { get; set; }

        public string HoTen { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        public string? DiaChi { get; set; }

        public string MatKhau { get; set; } = null!;

        public string VaiTro { get; set; } = "Khách hàng";
    }
}