
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly GasSparkDBEntities db = new GasSparkDBEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            base.OnActionExecuting(filterContext);
        }

        // Lọc sản phẩm theo danh mục và khoảng giá
        public ActionResult DanhMuc(
            int id,
            decimal? giaMin,
            decimal? giaMax,
            string sapXep = "macdinh")
        {
            var danhMuc = db.DanhMucs
                .FirstOrDefault(x => x.Id == id);

            var query = db.SanPhams
                .Where(x => x.IdDanhMuc == id);

            if (giaMin.HasValue && giaMin.Value >= 0)
            {
                query = query.Where(x => x.Gia >= giaMin.Value);
            }

            if (giaMax.HasValue && giaMax.Value >= 0)
            {
                query = query.Where(x => x.Gia <= giaMax.Value);
            }

            switch (sapXep)
            {
                case "tang":
                    query = query.OrderBy(x => x.Gia);
                    break;

                case "giam":
                    query = query.OrderByDescending(x => x.Gia);
                    break;

                default:
                    query = query.OrderBy(x => x.Id);
                    break;
            }

            ViewBag.TenDanhMuc = danhMuc != null
                ? danhMuc.TenDanhMuc
                : "Sản phẩm";

            ViewBag.GiaMin = giaMin;
            ViewBag.GiaMax = giaMax;
            ViewBag.SapXep = sapXep;

            return View(query.ToList());
        }
    }
}
