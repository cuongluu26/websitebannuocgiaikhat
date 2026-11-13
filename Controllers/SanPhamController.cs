
using System;
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly GasSparkDBEntities db =
            new GasSparkDBEntities();

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();

            base.OnActionExecuting(filterContext);
        }

        // Lọc sản phẩm theo danh mục, giá và phân trang
        public ActionResult DanhMuc(
            int id,
            decimal? giaMin,
            decimal? giaMax,
            string sapXep = "macdinh",
            int page = 1)
        {
            // Số sản phẩm mỗi trang
            int pageSize = 10;

            // Lấy danh mục
            var danhMuc = db.DanhMucs
                .FirstOrDefault(x => x.Id == id);

            // Lấy sản phẩm theo danh mục
            var query = db.SanPhams
                .Where(x => x.IdDanhMuc == id);

            // Lọc giá thấp nhất
            if (giaMin.HasValue && giaMin.Value >= 0)
            {
                query = query.Where(
                    x => x.Gia >= giaMin.Value);
            }

            // Lọc giá cao nhất
            if (giaMax.HasValue && giaMax.Value >= 0)
            {
                query = query.Where(
                    x => x.Gia <= giaMax.Value);
            }

            // Sắp xếp sản phẩm
            switch (sapXep)
            {
                case "tang":
                    query = query.OrderBy(x => x.Gia)
                                 .ThenBy(x => x.Id);
                    break;

                case "giam":
                    query = query.OrderByDescending(x => x.Gia)
                                 .ThenBy(x => x.Id);
                    break;

                default:
                    query = query.OrderBy(x => x.Id);
                    break;
            }

            // Đếm tổng sản phẩm sau khi lọc
            int totalProducts = query.Count();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize);

            // Kiểm tra trang hợp lệ
            page = Math.Max(1, Math.Min(
                page, Math.Max(1, totalPages)));

            // Lấy 10 sản phẩm của trang hiện tại
            var sanPhams = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Thông tin danh mục
            ViewBag.TenDanhMuc = danhMuc != null
                ? danhMuc.TenDanhMuc
                : "Sản phẩm";

            // Thông tin bộ lọc
            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;
            ViewBag.SapXep = sapXep;

            // Thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TongSoSanPham = totalProducts;

            return View(sanPhams);
        }
    }
}
