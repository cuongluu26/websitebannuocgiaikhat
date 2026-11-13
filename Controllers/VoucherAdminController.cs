using System;
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class VoucherAdminController : Controller
    {
        private readonly GasSparkDBEntities db = new GasSparkDBEntities();

        // !!! Sua cho nay cho khop cach website cua ban luu quyen admin luc dang nhap !!!
        // Mac dinh: Session["Role"] == "Admin"
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string role = Session["Role"] as string;
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = RedirectToAction("Login", "Account");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            return View(db.Vouchers.OrderByDescending(v => v.Id).ToList());
        }

        public ActionResult Create()
        {
            ViewBag.TieuDeTrang = "Thêm voucher";
            return View("Form", new Voucher
            {
                LoaiGiam = "PhanTram",
                NgayBatDau = DateTime.Now,
                NgayKetThuc = DateTime.Now.AddMonths(1),
                MoiNguoiToiDa = 1,
                TrangThai = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Voucher m)
        {
            Chuan(m);
            if (db.Vouchers.Any(x => x.Ma == m.Ma))
                ModelState.AddModelError("Ma", "Mã này đã tồn tại.");
            KiemTra(m);

            if (!ModelState.IsValid)
            {
                ViewBag.TieuDeTrang = "Thêm voucher";
                return View("Form", m);
            }

            m.DaDung = 0;
            db.Vouchers.Add(m);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var v = db.Vouchers.Find(id);
            if (v == null) return HttpNotFound();
            ViewBag.TieuDeTrang = "Sửa voucher";
            return View("Form", v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Voucher m)
        {
            var v = db.Vouchers.Find(m.Id);
            if (v == null) return HttpNotFound();

            Chuan(m);
            if (db.Vouchers.Any(x => x.Ma == m.Ma && x.Id != m.Id))
                ModelState.AddModelError("Ma", "Mã này đã tồn tại.");
            KiemTra(m);

            if (!ModelState.IsValid)
            {
                ViewBag.TieuDeTrang = "Sửa voucher";
                return View("Form", m);
            }

            v.Ma = m.Ma;
            v.TieuDe = m.TieuDe;
            v.MoTa = m.MoTa;
            v.LoaiGiam = m.LoaiGiam;
            v.GiaTri = m.GiaTri;
            v.DonToiThieu = m.DonToiThieu;
            v.GiamToiDa = m.GiamToiDa;
            v.NgayBatDau = m.NgayBatDau;
            v.NgayKetThuc = m.NgayKetThuc;
            v.SoLuong = m.SoLuong;
            v.MoiNguoiToiDa = m.MoiNguoiToiDa;
            v.TrangThai = m.TrangThai;
            // DaDung giu nguyen

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Da co nguoi dung thi chi tat (de giu lich su), chua dung thi xoa han
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var v = db.Vouchers.Find(id);
            if (v == null) return HttpNotFound();

            if (v.DaDung > 0 || db.VoucherUsages.Any(u => u.VoucherId == id))
            {
                v.TrangThai = false;
            }
            else
            {
                db.Vouchers.Remove(v);
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void Chuan(Voucher m)
        {
            m.Ma = (m.Ma ?? "").Trim().ToUpper().Replace(" ", "");
            m.TieuDe = (m.TieuDe ?? "").Trim();
        }

        private void KiemTra(Voucher m)
        {
            if (string.IsNullOrEmpty(m.Ma)) ModelState.AddModelError("Ma", "Nhập mã voucher.");
            if (string.IsNullOrEmpty(m.TieuDe)) ModelState.AddModelError("TieuDe", "Nhập tiêu đề.");
            if (m.LoaiGiam != "PhanTram" && m.LoaiGiam != "SoTien")
                ModelState.AddModelError("LoaiGiam", "Loại giảm không hợp lệ.");
            if (m.GiaTri <= 0) ModelState.AddModelError("GiaTri", "Giá trị giảm phải lớn hơn 0.");
            if (m.LoaiGiam == "PhanTram" && m.GiaTri > 100)
                ModelState.AddModelError("GiaTri", "Phần trăm không được quá 100.");
            if (m.NgayKetThuc <= m.NgayBatDau)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");
            if (m.MoiNguoiToiDa < 1)
                ModelState.AddModelError("MoiNguoiToiDa", "Tối thiểu 1 lần/người.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}