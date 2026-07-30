using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data; // Thư mục chứa ApplicationDbContext

namespace ShoeStore.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Tiêm (Inject) DbContext vào Controller
        public TrangChuController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult TrangChu()
        {
            // Lấy trực tiếp toàn bộ danh sách giày từ bảng Giay trong SQL Server
            var danhSachGiay = _db.Giay.ToList();

            // Trả về View cùng danh sách dữ liệu thật từ Database
            return View("~/Views/TrangChu/TrangChu.cshtml", danhSachGiay);
        }
    }
}