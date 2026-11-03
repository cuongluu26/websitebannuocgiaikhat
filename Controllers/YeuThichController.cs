using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class YeuThichController : Controller
    {
        GasSparkDBEntities db = new GasSparkDBEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            base.OnActionExecuting(filterContext);
        }

        // THÊM YÊU THÍCH
        public ActionResult Them(int id)
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            string username = Session["Username"].ToString();

            var exists = db.YeuThiches.FirstOrDefault(x => x.Username == username && x.IdSanPham == id);
            if (exists == null)
            {
                db.YeuThiches.Add(new YeuThich { Username = username, IdSanPham = id });
                db.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        // XÓA YÊU THÍCH
        public ActionResult Xoa(int id)
        {
            string username = Session["Username"].ToString();
            var item = db.YeuThiches.FirstOrDefault(x => x.Id == id && x.Username == username);
            if (item != null)
            {
                db.YeuThiches.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("DanhSach");
        }

        // DANH SÁCH YÊU THÍCH
        public ActionResult DanhSach()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            string username = Session["Username"].ToString();
            var list = db.YeuThiches
                         .Where(x => x.Username == username)
                         .ToList();

            return View(list);
        }
    }
}