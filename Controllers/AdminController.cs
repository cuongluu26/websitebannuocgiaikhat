using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class AdminController : Controller
    {
        private readonly GasSparkDBEntities db =
            new GasSparkDBEntities();

        private static readonly string[] DS_TRANG_THAI_DON =
        {
            "Chờ xác nhận",
            "Đang giao",
            "Hoàn thành",
            "Đã hủy"
        };

        // KIEM TRA QUYEN ADMIN (giu nguyen nhu ban dang chay duoc)
        private bool IsAdmin()
        {
            if (Session["Username"] == null) return false;

            var role = Session["Role"] as string;
            return string.Equals(role, "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            if (!IsAdmin())
            {
                filterContext.Result = RedirectToAction(
                    "Login", "Account");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        // =====================================================
        // DASHBOARD  ->  Views/Admin/Index.cshtml
        // So lieu truyen qua ViewBag
        // =====================================================
        public ActionResult Index()
        {
            ViewBag.TongSanPham = db.SanPhams.Count();
            ViewBag.TongDanhMuc = db.DanhMucs.Count();
            ViewBag.TongDonHang = db.DonHangs.Count();
            ViewBag.TongUser = db.Users.Count();
            ViewBag.LienHeChuaDoc = db.LienHes.Count(x => x.DaDoc == false);

            ViewBag.DoanhThu = db.DonHangs
                .Where(x => x.TrangThai == "Hoàn thành")
                .Select(x => (decimal?)x.TongTien)
                .Sum() ?? 0;

            ViewBag.ChoThanhToan = db.DonHangs.Count(x =>
                x.PhuongThucThanhToan == "Banking" &&
                x.TrangThaiThanhToan != "Đã thanh toán");

            return View();
        }

        // =====================================================
        // SAN PHAM  ->  SanPham / CreateSanPham / EditSanPham
        // =====================================================
        public ActionResult SanPham(string keyword, int? danhMucId)
        {
            var query = db.SanPhams
                          .Include(x => x.DanhMuc)
                          .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.TenSanPham.Contains(keyword) ||
                    x.ThuongHieu.Contains(keyword));
            }

            if (danhMucId.HasValue)
            {
                query = query.Where(x => x.IdDanhMuc == danhMucId.Value);
            }

            ViewBag.DanhMuc = db.DanhMucs.ToList();

            return View(query.OrderByDescending(x => x.Id).ToList());
        }

        public ActionResult CreateSanPham()
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            return View("CreateSanPham", new SanPham());
        }

        [HttpPost]
        public ActionResult CreateSanPham(SanPham sp)
        {
            if (string.IsNullOrWhiteSpace(sp.TenSanPham))
            {
                ModelState.AddModelError("TenSanPham",
                    "Vui long nhap ten san pham.");
            }

            if (sp.Gia < 0)
            {
                ModelState.AddModelError("Gia", "Gia khong hop le.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DanhMuc = db.DanhMucs.ToList();
                return View("CreateSanPham", sp);
            }

            var tenFile = LuuHinhAnh();
            if (tenFile != null) sp.HinhAnh = tenFile;

            db.SanPhams.Add(sp);
            db.SaveChanges();

            return RedirectToAction("SanPham");
        }

        public ActionResult EditSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            ViewBag.DanhMuc = db.DanhMucs.ToList();
            return View("EditSanPham", sp);
        }

        [HttpPost]
        public ActionResult EditSanPham(SanPham sp)
        {
            if (string.IsNullOrWhiteSpace(sp.TenSanPham))
            {
                ModelState.AddModelError("TenSanPham",
                    "Vui long nhap ten san pham.");
            }

            if (sp.Gia < 0)
            {
                ModelState.AddModelError("Gia", "Gia khong hop le.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DanhMuc = db.DanhMucs.ToList();
                return View("EditSanPham", sp);
            }

            var goc = db.SanPhams.Find(sp.Id);
            if (goc == null) return HttpNotFound();

            goc.TenSanPham = sp.TenSanPham;
            goc.Gia = sp.Gia;
            goc.ThuongHieu = sp.ThuongHieu;
            goc.IdDanhMuc = sp.IdDanhMuc;
            goc.Loai = sp.Loai;
            goc.MoTa = sp.MoTa;
            goc.DungTich = sp.DungTich;
            goc.ThanhPhan = sp.ThanhPhan;
            goc.XuatXu = sp.XuatXu;
            goc.HanSuDung = sp.HanSuDung;

            var tenFile = LuuHinhAnh();
            if (tenFile != null) goc.HinhAnh = tenFile;

            db.SaveChanges();

            return RedirectToAction("SanPham");
        }

        public ActionResult XoaSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            try
            {
                db.SanPhams.Remove(sp);
                db.SaveChanges();
                TempData["Msg"] = "Da xoa san pham.";
            }
            catch (Exception)
            {
                TempData["Msg"] =
                    "Khong the xoa: san pham dang duoc su dung.";
            }

            return RedirectToAction("SanPham");
        }

        // =====================================================
        // DANH MUC  ->  DanhMuc / CreateDanhMuc / EditDanhMuc
        // =====================================================
        public ActionResult DanhMuc()
        {
            return View(db.DanhMucs
                          .OrderBy(x => x.ThuTu)
                          .ThenBy(x => x.Id)
                          .ToList());
        }

        public ActionResult CreateDanhMuc()
        {
            return View("CreateDanhMuc", new DanhMuc());
        }

        [HttpPost]
        public ActionResult CreateDanhMuc(DanhMuc dm)
        {
            if (string.IsNullOrWhiteSpace(dm.TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc",
                    "Vui long nhap ten danh muc.");
            }

            if (!ModelState.IsValid)
            {
                return View("CreateDanhMuc", dm);
            }

            var tenFile = LuuHinhAnh();
            if (tenFile != null) dm.HinhAnh = tenFile;

            db.DanhMucs.Add(dm);
            db.SaveChanges();

            return RedirectToAction("DanhMuc");
        }

        public ActionResult EditDanhMuc(int id)
        {
            var dm = db.DanhMucs.Find(id);
            if (dm == null) return HttpNotFound();

            return View("EditDanhMuc", dm);
        }

        [HttpPost]
        public ActionResult EditDanhMuc(DanhMuc dm)
        {
            if (string.IsNullOrWhiteSpace(dm.TenDanhMuc))
            {
                ModelState.AddModelError("TenDanhMuc",
                    "Vui long nhap ten danh muc.");
            }

            if (!ModelState.IsValid)
            {
                return View("EditDanhMuc", dm);
            }

            var goc = db.DanhMucs.Find(dm.Id);
            if (goc == null) return HttpNotFound();

            goc.TenDanhMuc = dm.TenDanhMuc;
            goc.MoTa = dm.MoTa;
            goc.ThuTu = dm.ThuTu;
            goc.TrangThai = dm.TrangThai;

            var tenFile = LuuHinhAnh();
            if (tenFile != null) goc.HinhAnh = tenFile;

            db.SaveChanges();

            return RedirectToAction("DanhMuc");
        }

        public ActionResult XoaDanhMuc(int id)
        {
            var dm = db.DanhMucs.Find(id);
            if (dm == null) return HttpNotFound();

            try
            {
                db.DanhMucs.Remove(dm);
                db.SaveChanges();
                TempData["Msg"] = "Da xoa danh muc.";
            }
            catch (Exception)
            {
                TempData["Msg"] =
                    "Khong the xoa: danh muc dang co san pham.";
            }

            return RedirectToAction("DanhMuc");
        }

        // =====================================================
        // DON HANG  ->  DonHang / ChiTietDonHang
        // =====================================================
        public ActionResult DonHang(string keyword, string trangThai)
        {
            var query = db.DonHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.HoTen.Contains(keyword) ||
                    x.SoDienThoai.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            return View(query.OrderByDescending(x => x.NgayDat)
                             .ThenByDescending(x => x.Id)
                             .ToList());
        }

        // Model = don hang, danh sach san pham trong ViewBag.ChiTiet
        public ActionResult ChiTietDonHang(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            ViewBag.ChiTiet = db.ChiTietDonHangs
                                .Where(x => x.IdDonHang == id)
                                .ToList();

            return View(donHang);
        }

        // Admin xac nhan da nhan tien chuyen khoan (don Banking)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanThanhToan(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            if (donHang.PhuongThucThanhToan != "Banking")
            {
                TempData["Msg"] =
                    "Don hang nay khong thanh toan chuyen khoan.";
                return RedirectToAction("DonHang");
            }

            if (donHang.TrangThaiThanhToan != "Đã thanh toán")
            {
                donHang.TrangThaiThanhToan = "Đã thanh toán";
                db.SaveChanges();
            }

            TempData["Msg"] = "Da xac nhan thanh toan don #" + id;
            return RedirectToAction("DonHang");
        }

        public ActionResult CapNhatTrangThaiDonHang(int id, string trangThai)
        {
            if (!DS_TRANG_THAI_DON.Contains(trangThai))
            {
                return new HttpStatusCodeResult(400);
            }

            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThai = trangThai;
            db.SaveChanges();

            return RedirectToAction("DonHang");
        }

        public ActionResult XoaDonHang(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var chiTiet = db.ChiTietDonHangs
                                    .Where(x => x.IdDonHang == id)
                                    .ToList();

                    db.ChiTietDonHangs.RemoveRange(chiTiet);
                    db.DonHangs.Remove(donHang);
                    db.SaveChanges();

                    transaction.Commit();
                    TempData["Msg"] = "Da xoa don hang #" + id;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    TempData["Msg"] = "Khong the xoa don hang.";
                }
            }

            return RedirectToAction("DonHang");
        }

        // =====================================================
        // NGUOI DUNG  ->  User.cshtml
        // Ten method khac de khong de len Controller.User
        // =====================================================
        [ActionName("User")]
        public ActionResult UserList()
        {
            return View("User", db.Users.OrderBy(x => x.Id).ToList());
        }

        public ActionResult XoaUser(int id)
        {
            var u = db.Users.Find(id);
            if (u == null) return HttpNotFound();

            // Khong cho admin tu xoa chinh minh
            var hienTai = Session["Username"] as string;
            if (string.Equals(u.Username, hienTai,
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["Msg"] = "Khong the xoa tai khoan dang dang nhap.";
                return RedirectToAction("User");
            }

            try
            {
                db.Users.Remove(u);
                db.SaveChanges();
                TempData["Msg"] = "Da xoa nguoi dung.";
            }
            catch (Exception)
            {
                TempData["Msg"] = "Khong the xoa nguoi dung.";
            }

            return RedirectToAction("User");
        }

        // =====================================================
        // LIEN HE  ->  LienHe.cshtml
        // =====================================================
        public ActionResult LienHe()
        {
            return View(db.LienHes
                          .OrderByDescending(x => x.NgayGui)
                          .ToList());
        }

        public ActionResult DanhDauDaDoc(int id)
        {
            var lh = db.LienHes.Find(id);
            if (lh == null) return HttpNotFound();

            lh.DaDoc = true;
            db.SaveChanges();

            return RedirectToAction("LienHe");
        }

        public ActionResult XoaLienHe(int id)
        {
            var lh = db.LienHes.Find(id);
            if (lh == null) return HttpNotFound();

            db.LienHes.Remove(lh);
            db.SaveChanges();

            return RedirectToAction("LienHe");
        }

        // =====================================================
        // LUU HINH ANH UPLOAD VAO ~/Images
        // =====================================================
        private string LuuHinhAnh()
        {
            if (Request.Files == null || Request.Files.Count == 0)
                return null;

            var file = Request.Files[0];
            if (file == null || file.ContentLength == 0)
                return null;

            var ext = Path.GetExtension(file.FileName)?.ToLower();
            var hopLe = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (ext == null || !hopLe.Contains(ext))
                return null;

            var ten = Guid.NewGuid().ToString("N") + ext;
            var duongDan = Path.Combine(Server.MapPath("~/Images"), ten);
            file.SaveAs(duongDan);

            return ten;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}