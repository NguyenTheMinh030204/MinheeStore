using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data; 

namespace ShoeStore.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TrangChuController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult TrangChu()
        {
            
            var danhSachGiay = _db.Giay.ToList();

            return View("~/Views/TrangChu/TrangChu.cshtml", danhSachGiay);
        }
    }
}
