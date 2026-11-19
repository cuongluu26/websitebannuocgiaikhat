using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class VoucherAdminController : Controller
    {
        private readonly GasSparkDBEntities db = new GasSparkDBEntities();

        // ===== Cấu hình ảnh voucher =====
        private const string THU_MUC_ANH = "~/Content/Images/KhuyenMai/";
        private const int ANH_TOI_DA = 2 * 1024 * 1024;               // 2MB
        private static readonly string[] ANH_HOP_LE = { ".jpg", ".jpeg", ".png", ".webp" };

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
        public ActionResult Create(Voucher m, HttpPostedFileBase fileAnh)
        {
            Chuan(m);
            if (db.Vouchers.Any(x => x.Ma == m.Ma))
                ModelState.AddModelError("Ma", "Mã này đã tồn tại.");
            KiemTra(m);
            KiemTraAnh(fileAnh);

            if (!ModelState.IsValid)
            {
                m.HinhAnh = null; // ảnh chưa lưu, người dùng chọn lại
                ViewBag.TieuDeTrang = "Thêm voucher";
                return View("Form", m);
            }

            m.DaDung = 0;
            m.HinhAnh = LuuAnh(fileAnh); // null nếu không chọn ảnh
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
        public ActionResult Edit(Voucher m, HttpPostedFileBase fileAnh, bool xoaAnh = false)
        {
            var v = db.Vouchers.Find(m.Id);
            if (v == null) return HttpNotFound();

            Chuan(m);
            if (db.Vouchers.Any(x => x.Ma == m.Ma && x.Id != m.Id))
                ModelState.AddModelError("Ma", "Mã này đã tồn tại.");
            KiemTra(m);
            KiemTraAnh(fileAnh);

            if (!ModelState.IsValid)
            {
                m.HinhAnh = v.HinhAnh; // giữ hiển thị ảnh hiện tại
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

            // ===== Ảnh =====
            string anhCu = v.HinhAnh;
            string anhMoi = LuuAnh(fileAnh);
            bool doiAnh = false;

            if (anhMoi != null)            // chọn ảnh mới -> thay
            {
                v.HinhAnh = anhMoi;
                doiAnh = true;
            }
            else if (xoaAnh)               // tick "Xóa ảnh" -> bỏ ảnh
            {
                v.HinhAnh = null;
                doiAnh = true;
            }
            // còn lại: giữ nguyên ảnh cũ

            db.SaveChanges();

            if (doiAnh) XoaAnh(anhCu);     // xóa file cũ sau khi lưu DB thành công
            return RedirectToAction("Index");
        }

        // Da co nguoi dung thi chi tat (de giu lich su), chua dung thi xoa han
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var v = db.Vouchers.Find(id);
            if (v == null) return HttpNotFound();

            string anh = null;

            if (v.DaDung > 0 || db.VoucherUsages.Any(u => u.VoucherId == id))
            {
                v.TrangThai = false;
            }
            else
            {
                anh = v.HinhAnh;
                db.Vouchers.Remove(v);
            }

            db.SaveChanges();
            XoaAnh(anh);
            return RedirectToAction("Index");
        }

        // =====================================================
        //  ẢNH
        // =====================================================

        // Kiểm tra: đuôi file, dung lượng, và nội dung thật sự là ảnh
        private void KiemTraAnh(HttpPostedFileBase f)
        {
            if (f == null || f.ContentLength == 0) return;

            string ext = Path.GetExtension(f.FileName ?? "").ToLowerInvariant();

            if (!ANH_HOP_LE.Contains(ext))
                ModelState.AddModelError("HinhAnh", "Ảnh chỉ nhận định dạng JPG, PNG hoặc WEBP.");
            else if (f.ContentLength > ANH_TOI_DA)
                ModelState.AddModelError("HinhAnh", "Ảnh tối đa 2MB.");
            else if (!LaAnhThat(f))
                ModelState.AddModelError("HinhAnh", "File không phải ảnh hợp lệ.");
        }

        // Đọc vài byte đầu để chắc chắn đúng là JPG / PNG / WEBP (không tin vào đuôi file)
        private bool LaAnhThat(HttpPostedFileBase f)
        {
            try
            {
                var head = new byte[12];
                f.InputStream.Position = 0;
                int n = f.InputStream.Read(head, 0, head.Length);
                f.InputStream.Position = 0;
                if (n < 12) return false;

                bool jpg = head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
                bool png = head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
                bool webp = head[0] == 'R' && head[1] == 'I' && head[2] == 'F' && head[3] == 'F'
                         && head[8] == 'W' && head[9] == 'E' && head[10] == 'B' && head[11] == 'P';
                return jpg || png || webp;
            }
            catch { return false; }
        }

        // Lưu file với tên ngẫu nhiên, trả về tên file (hoặc null nếu không có ảnh)
        private string LuuAnh(HttpPostedFileBase f)
        {
            if (f == null || f.ContentLength == 0) return null;

            string ext = Path.GetExtension(f.FileName).ToLowerInvariant();
            string dir = Server.MapPath(THU_MUC_ANH);
            Directory.CreateDirectory(dir);

            string ten = "vc_" + Guid.NewGuid().ToString("N") + ext;
            f.SaveAs(Path.Combine(dir, ten));
            return ten;
        }

        private void XoaAnh(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return;
            try
            {
                // GetFileName chặn kiểu "../../" đi ra ngoài thư mục
                string path = Path.Combine(Server.MapPath(THU_MUC_ANH), Path.GetFileName(ten));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            catch { /* không để lỗi xóa file làm hỏng thao tác chính */ }
        }

        // =====================================================

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