using System;
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private readonly GasSparkDBEntities db = new GasSparkDBEntities();

        private const int PAGE_SIZE = 8; // 4 cột x 2 hàng

        // /KhuyenMai/Index?page=1
        public ActionResult Index(int page = 1)
        {
            var now = DateTime.Now;

            var query = db.Vouchers
                .Where(v => v.TrangThai
                            && v.NgayKetThuc >= now
                            && (v.SoLuong == null || v.DaDung < v.SoLuong))
                .OrderBy(v => v.NgayKetThuc);

            int total = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)PAGE_SIZE);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var ds = query
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(ds);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}