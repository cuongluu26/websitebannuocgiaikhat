
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class CartController : Controller
    {
        private const string CART_KEY = "GioHang";

        private readonly GasSparkDBEntities db =
            new GasSparkDBEntities();

        private const string BANK_ID = "VCB";
        private const string BANK_ACCOUNT = "1026771542";
        private const string BANK_ACCOUNT_NAME = "LUU HUNG CUONG";

        private List<CartItem> GetCart()
        {
            var cart = Session[CART_KEY] as List<CartItem>;

            if (cart == null)
            {
                cart = new List<CartItem>();
                Session[CART_KEY] = cart;
            }

            return cart;
        }

        // GIO HANG
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction(
                    "Login", "Account",
                    new { returnUrl = Url.Action("Index", "Cart") }
                );
            }

            return View(GetCart());
        }

        // THEM SAN PHAM
        [HttpPost]
        public JsonResult ThemVaoGio(
            int id, string ten, string hinh,
            decimal gia, int soLuong = 1)
        {
            if (Session["Username"] == null)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true
                });
            }

            if (soLuong < 1 || soLuong > 100)
            {
                return Json(new
                {
                    success = false,
                    message = "So luong khong hop le."
                });
            }

            var cart = GetCart();

            var item = cart.FirstOrDefault(
                x => x.SanPhamId == id
            );

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    SanPhamId = id,
                    TenSanPham = ten,
                    HinhAnh = hinh,
                    DonGia = gia,
                    SoLuong = soLuong
                });
            }
            else
            {
                if (item.SoLuong + soLuong > 100)
                {
                    return Json(new
                    {
                        success = false,
                        message = "So luong vuot gioi han."
                    });
                }

                item.SoLuong += soLuong;
            }

            return Json(new
            {
                success = true,
                message = "Da them vao gio hang!",
                tongSoLuong = cart.Sum(x => x.SoLuong)
            });
        }

        // XOA SAN PHAM
        [HttpPost]
        public JsonResult XoaSanPham(int id)
        {
            var cart = Session[CART_KEY] as List<CartItem>
                       ?? new List<CartItem>();

            cart.RemoveAll(x => x.SanPhamId == id);

            Session[CART_KEY] = cart;

            return Json(new
            {
                success = true,
                tongTien = cart.Sum(x => x.ThanhTien),
                tongSoLuong = cart.Sum(x => x.SoLuong)
            });
        }

        // CAP NHAT SO LUONG
        [HttpPost]
        public JsonResult CapNhatSoLuong(int id, int soLuong)
        {
            var cart = Session[CART_KEY] as List<CartItem>
                       ?? new List<CartItem>();

            if (soLuong < 0 || soLuong > 100)
            {
                return Json(new
                {
                    success = false,
                    message = "So luong khong hop le."
                });
            }

            decimal thanhTien = 0;

            if (soLuong == 0)
            {
                cart.RemoveAll(x => x.SanPhamId == id);
            }
            else
            {
                var item = cart.FirstOrDefault(
                    x => x.SanPhamId == id
                );

                if (item != null)
                {
                    item.SoLuong = soLuong;
                    thanhTien = item.ThanhTien;
                }
            }

            Session[CART_KEY] = cart;

            return Json(new
            {
                success = true,
                thanhTien = thanhTien,
                tongTien = cart.Sum(x => x.ThanhTien),
                tongSoLuong = cart.Sum(x => x.SoLuong)
            });
        }

        // XOA TAT CA
        [HttpPost]
        public JsonResult XoaTatCa()
        {
            Session.Remove(CART_KEY);

            return Json(new { success = true });
        }

        // CHECKOUT
        public ActionResult Checkout()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        // DAT HANG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatHang(
            string hoTen,
            string soDienThoai,
            string diaChi,
            string ghiChu,
            string phuongThuc)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(soDienThoai) ||
                string.IsNullOrWhiteSpace(diaChi))
            {
                ModelState.AddModelError(
                    "",
                    "Vui long nhap day du thong tin giao hang."
                );

                return View("Checkout", cart);
            }

            if (phuongThuc != "COD" &&
                phuongThuc != "Banking")
            {
                return new HttpStatusCodeResult(400);
            }

            if (cart.Any(x => x.SoLuong < 1 || x.DonGia < 0))
            {
                return new HttpStatusCodeResult(400);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var donHang = new DonHang
                    {
                        Username = Session["Username"].ToString(),
                        HoTen = hoTen.Trim(),
                        SoDienThoai = soDienThoai.Trim(),
                        DiaChi = diaChi.Trim(),
                        GhiChu = ghiChu,
                        TongTien = cart.Sum(x => x.ThanhTien),
                        NgayDat = DateTime.Now,
                        TrangThai = "Chờ xác nhận",

                        PhuongThucThanhToan = phuongThuc,
                        TrangThaiThanhToan = "Chưa thanh toán",
                        NoiDungChuyenKhoan = null
                    };

                    db.DonHangs.Add(donHang);
                    db.SaveChanges();

                    // Tao ma chuyen khoan theo ID don hang
                    if (phuongThuc == "Banking")
                    {
                        donHang.NoiDungChuyenKhoan =
                            "GAS" + donHang.Id;

                        db.SaveChanges();
                    }

                    // LUU CHI TIET DON HANG
                    foreach (var item in cart)
                    {
                        db.ChiTietDonHangs.Add(
                            new ChiTietDonHang
                            {
                                IdDonHang = donHang.Id,
                                IdSanPham = item.SanPhamId,
                                TenSanPham = item.TenSanPham,
                                HinhAnh = item.HinhAnh,
                                DonGia = item.DonGia,
                                SoLuong = item.SoLuong
                            }
                        );
                    }

                    db.SaveChanges();

                    int idDonHang = donHang.Id;

                    transaction.Commit();

                    Session.Remove(CART_KEY);

                    if (phuongThuc == "Banking")
                    {
                        return RedirectToAction(
                            "ThanhToanNganHang",
                            new { id = idDonHang }
                        );
                    }

                    return RedirectToAction(
                        "DatHangThanhCong",
                        new { id = idDonHang }
                    );
                }
                catch
                {
                    transaction.Rollback();

                    return new HttpStatusCodeResult(
                        500,
                        "Khong the tao don hang. Vui long thu lai."
                    );
                }
            }
        }

        // TRANG THANH TOAN VIETQR
        public ActionResult ThanhToanNganHang(int id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();

            var donHang = db.DonHangs.FirstOrDefault(
                x => x.Id == id &&
                     x.Username == username
            );

            if (donHang == null)
            {
                return HttpNotFound();
            }

            if (donHang.PhuongThucThanhToan != "Banking")
            {
                return new HttpStatusCodeResult(400);
            }

            if (donHang.TrangThaiThanhToan == "Đã thanh toán")
            {
                return RedirectToAction(
                    "DatHangThanhCong",
                    new { id = donHang.Id }
                );
            }

            if (string.IsNullOrWhiteSpace(BANK_ACCOUNT) ||
                BANK_ACCOUNT == "THAY_SO_TAI_KHOAN")
            {
                return new HttpStatusCodeResult(
                    500,
                    "Chua cau hinh tai khoan ngan hang."
                );
            }

            ViewBag.BankId = BANK_ID;
            ViewBag.Account = BANK_ACCOUNT;
            ViewBag.AccountName = BANK_ACCOUNT_NAME;

            ViewBag.QrUrl =
                "https://img.vietqr.io/image/" +
                Uri.EscapeDataString(BANK_ID) + "-" +
                Uri.EscapeDataString(BANK_ACCOUNT) +
                "-compact2.png?amount=" +
                Uri.EscapeDataString(
                    donHang.TongTien.ToString("0")
                ) +
                "&addInfo=" +
                Uri.EscapeDataString(
                    donHang.NoiDungChuyenKhoan ?? ""
                ) +
                "&accountName=" +
                Uri.EscapeDataString(BANK_ACCOUNT_NAME);

            return View(donHang);
        }
        // TRANG QR HOI TRANG THAI THANH TOAN
        [HttpGet]
        public JsonResult KiemTraThanhToan(int id)
        {
            if (Session["Username"] == null)
            {
                return Json(new { paid = false }, JsonRequestBehavior.AllowGet);
            }

            string username = Session["Username"].ToString();

            bool paid = db.DonHangs.Any(x =>
                x.Id == id &&
                x.Username == username &&
                x.TrangThaiThanhToan == "Đã thanh toán");

            return Json(new { paid = paid }, JsonRequestBehavior.AllowGet);
        }
        // DAT HANG THANH CONG
        public ActionResult DatHangThanhCong(int id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string username = Session["Username"].ToString();

            var donHang = db.DonHangs.FirstOrDefault(
                x => x.Id == id &&
                     x.Username == username
            );

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
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
