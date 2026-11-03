using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class HomeController : Controller
    {
        GasSparkDBEntities db = new GasSparkDBEntities();

        // ===== LOAD MENU DANH MỤC =====
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.DanhMuc = db.DanhMucs.ToList();
            base.OnActionExecuting(filterContext);
        }

        // ===== TRANG CHÍNH =====
        public ActionResult Tranggiaodien(int page = 1)
        {
            int pageSize = 4;

            var data = db.SanPhams
                         .OrderBy(x => Guid.NewGuid()) // ← random
                         .Take(pageSize)
                         .ToList();

            return View(data);
        }
        // ===== LOAD MORE =====
        public ActionResult LoadMore(int page)
        {
            int pageSize = 4;

            var data = db.SanPhams
                         .OrderBy(x => x.Id)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

            if (!data.Any())
                return Content("");

            return PartialView("_SanPhamPartial", data);
        }

        // ===== CHI TIẾT =====
        public ActionResult SanPham(int id)
        {
            var sp = db.SanPhams.FirstOrDefault(x => x.Id == id);
            if (sp == null) return HttpNotFound();
            return View(sp);
        }

        // ===== LỌC THEO DANH MỤC =====
        public ActionResult DanhMuc(int id)
        {
            var list = db.SanPhams
                         .Where(x => x.IdDanhMuc == id)
                         .ToList();

            return View("Tranggiaodien", list);
        }
        // ===== TÌM KIẾM =====
        public ActionResult TimKiem(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction("Tranggiaodien");

            var ketQua = db.SanPhams
                           .Where(x => x.TenSanPham.Contains(q) ||
                                        x.MoTa.Contains(q) ||
                                        x.ThuongHieu.Contains(q))
                           .ToList();

            ViewBag.TuKhoa = q;
            ViewBag.SoKetQua = ketQua.Count;
            return View(ketQua);
        }
        public ActionResult GoiY(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            var ketQua = db.SanPhams
                           .Where(x => x.TenSanPham.Contains(q) ||
                                        x.ThuongHieu.Contains(q))
                           .Take(6)
                           .Select(x => new {
                               x.Id,
                               x.TenSanPham,
                               x.Gia,
                               x.HinhAnh
                           })
                           .ToList();

            return Json(ketQua, JsonRequestBehavior.AllowGet);
        }
        // ===== LIÊN HỆ =====
        public ActionResult LienHe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LienHe(string hoTen, string email, string soDienThoai, string tieuDe, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(noiDung))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ thông tin bắt buộc!";
                return View();
            }

            var lh = new LienHe
            {
                HoTen = hoTen,
                Email = email,
                SoDienThoai = soDienThoai,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayGui = DateTime.Now,
                DaDoc = false
            };

            db.LienHes.Add(lh);
            db.SaveChanges();

            ViewBag.Success = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return View();
        }
        public ActionResult ChinhSachGiaoHang() => View();
        public ActionResult ChinhSachKiemHang() => View();
        public ActionResult ChinhSachDoiTra() => View();
        public ActionResult ChinhSachThanhToan() => View();
        public ActionResult QuyMoVaThiTruong() => View();
        public ActionResult TamNhin() => View();
        public ActionResult ChungNhanChatLuong() => View();
    }
}