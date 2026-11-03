using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class DonhangController : Controller
    {
        private GasSparkDBEntities db = new GasSparkDBEntities();

        // GET: /DonHang/LichSu
        public ActionResult LichSu()
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("DangNhap", "Account");

            var donHangs = db.DonHangs
                .Where(d => d.Username == username)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            var donHangIds = donHangs.Select(d => d.Id).ToList();
            ViewBag.AllChiTiets = db.ChiTietDonHangs
                .Where(c => donHangIds.Contains(c.IdDonHang))
                .ToList();

            return View(donHangs);
        }

        // GET: /DonHang/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("DangNhap", "Account");

            var donHang = db.DonHangs
                .FirstOrDefault(d => d.Id == id && d.Username == username);

            if (donHang == null)
                return HttpNotFound();

            ViewBag.ChiTiets = db.ChiTietDonHangs
                .Where(c => c.IdDonHang == id)
                .ToList();

            return View(donHang);
        }
    }
}