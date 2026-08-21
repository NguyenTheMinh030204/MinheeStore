using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoeStore.Controllers
{
    public class TimKiemController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TimKiemController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult TimKiemNhanh(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { success = false, total = 0, data = new List<object>() });
            }

            string kw = keyword.Trim().ToLower();

            var query = _db.Giay.Where(p => p.TenGiay.ToLower().Contains(kw));

            int totalMatches = query.Count();

            var items = query.Take(5).Select(p => new
            {
                id = p.MaGiay,
                ten = p.TenGiay,
                anh = p.AnhChinh,
                giaBan = p.GiaBan,
                giaCu = p.GiaCu
            }).ToList();

            return Json(new
            {
                success = true,
                total = totalMatches,
                data = items
            });
        }

        [HttpGet]
        public IActionResult KetQua(string search = "", int page = 1)
        {
            int pageSize = 20; 
            string kw = (search ?? "").Trim().ToLower();

            var query = _db.Giay.AsQueryable();

            if (!string.IsNullOrEmpty(kw))
            {
                query = query.Where(p => p.TenGiay.ToLower().Contains(kw));
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;

            var dsGiay = query.OrderByDescending(p => p.MaGiay)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            ViewBag.TuKhoa = search;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(dsGiay);
        }
    }
}
