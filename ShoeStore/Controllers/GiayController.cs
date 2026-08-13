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

        // GET: /Giay/ChiTiet?id=G03022004-001
        [HttpGet]
        public async Task<IActionResult> ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("TrangChu", "TrangChu");
            }

            // Query Chi tiết Giày + Join DanhMuc + Join Thư viện Ảnh + Join Biến thể Size/Tồn kho
            var giay = await _db.Giay
                .Include(g => g.DanhMuc)
                .Include(g => g.AnhGiay)       // Bảng AnhGiay (Ảnh phụ)
                .Include(g => g.BienTheGiay)   // Bảng BienTheGiay (KichCo, SoLuongTon)
                .FirstOrDefaultAsync(g => g.MaGiay == id);

            if (giay == null)
            {
                return NotFound();
            }

            // Lấy 4 sản phẩm liên quan cùng Danh mục (trừ sản phẩm hiện tại)
            var dsLienQuan = await _db.Giay
                .Where(g => g.MaDanhMuc == giay.MaDanhMuc && g.MaGiay != giay.MaGiay)
                .Take(4)
                .ToListAsync();

            ViewBag.SanPhamLienQuan = dsLienQuan;

            return View(giay);
        }
    }
}