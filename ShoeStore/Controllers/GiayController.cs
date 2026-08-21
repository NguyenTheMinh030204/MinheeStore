using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ShoeStore.Controllers
{
    public class GiayController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GiayController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("TrangChu", "TrangChu");
            }

            var giay = await _db.Giay
                .Include(g => g.DanhMuc)
                .Include(g => g.AnhGiay)       
                .Include(g => g.BienTheGiay)   
                .FirstOrDefaultAsync(g => g.MaGiay == id);

            if (giay == null)
            {
                return NotFound();
            }

            var dsLienQuan = await _db.Giay
                .Where(g => g.MaDanhMuc == giay.MaDanhMuc && g.MaGiay != giay.MaGiay)
                .Take(4)
                .ToListAsync();

            ViewBag.SanPhamLienQuan = dsLienQuan;

            return View(giay);
        }
    }
}
