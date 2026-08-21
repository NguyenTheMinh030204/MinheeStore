using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoeStore.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DanhMucController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult DanhSach(string id = "", int page = 1, string sort = "", [FromQuery] List<string> price = null, [FromQuery] List<int> size = null)
        {
            int pageSize = 12;

            price = price ?? new List<string>();
            size = size ?? new List<int>();

            var query = _db.Giay.AsQueryable();

            if (id == "HotSale")
            {
                query = query.Where(p => p.LaHotSale);
                ViewBag.TenDanhMuc = "HOT SALE";
            }
            else if (!string.IsNullOrEmpty(id))
            {
                query = query.Where(p => p.MaDanhMuc == id);
                var danhMuc = _db.DanhMuc.FirstOrDefault(d => d.MaDanhMuc == id);
                ViewBag.TenDanhMuc = danhMuc != null ? danhMuc.TenDanhMuc : "TẤT CẢ SẢN PHẨM";
            }
            else
            {
                ViewBag.TenDanhMuc = "TẤT CẢ SẢN PHẨM";
            }

            if (price.Count > 0)
            {
                query = query.Where(p =>
                    (price.Contains("0-500") && p.GiaBan < 500000) ||
                    (price.Contains("500-1000") && p.GiaBan >= 500000 && p.GiaBan <= 1000000) ||
                    (price.Contains("1000-2000") && p.GiaBan > 1000000 && p.GiaBan <= 2000000) ||
                    (price.Contains("2000-up") && p.GiaBan > 2000000)
                );
            }

            if (size.Count > 0)
            {
                query = query.Where(p => _db.BienTheGiay.Any(bt => bt.MaGiay == p.MaGiay && size.Contains(bt.KichCo)));
            }

            switch (sort)
            {
                case "price-asc":
                    query = query.OrderBy(p => p.GiaBan).ThenBy(p => p.MaGiay);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(p => p.GiaBan).ThenBy(p => p.MaGiay);
                    break;
                case "name-asc":
                    query = query.OrderBy(p => p.TenGiay).ThenBy(p => p.MaGiay);
                    break;
                case "name-desc":
                    query = query.OrderByDescending(p => p.TenGiay).ThenBy(p => p.MaGiay);
                    break;
                default:
                    query = query.OrderByDescending(p => p.MaGiay);
                    break;
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var dsGiay = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentMaDanhMuc = id;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSort = sort;
            ViewBag.SelectedPrices = price;
            ViewBag.SelectedSizes = size;

            return View(dsGiay);
        }
    }
}
