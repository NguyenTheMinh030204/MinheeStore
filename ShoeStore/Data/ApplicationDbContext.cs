using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;

namespace ShoeStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<Giay> Giay { get; set; }
        public DbSet<AnhGiay> AnhGiay { get; set; }
        public DbSet<BienTheGiay> BienTheGiay { get; set; }
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<GioHang> GioHang { get; set; }
        public DbSet<ChiTietGioHang> ChiTietGioHang { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}