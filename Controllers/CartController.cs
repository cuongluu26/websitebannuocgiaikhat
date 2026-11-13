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
        private const string VOUCHER_KEY = "VoucherCode";

        private readonly GasSparkDBEntities db =
            new GasSparkDBEntities();

        private const string BANK_ID = "VPB";
        private const string BANK_ACCOUNT = "0000000001";
        private const string BANK_ACCOUNT_NAME = "Le Tien P";

        // Dung de bao voucher vua het luot trong luc dat hang
        private class VoucherHetLuotException : Exception { }

        // ============================================================
        //  GIO HANG (SESSION + SQL)
        // ============================================================

        // Lấy giỏ: ưu tiên Session, chưa có thì nạp từ SQL theo tài khoản
        private List<CartItem> GetCart()
        {
            var cart = Session[CART_KEY] as List<CartItem>;
            if (cart != null) return cart;

            cart = new List<CartItem>();
            string username = Session["Username"] as string;
            if (!string.IsNullOrEmpty(username))
            {
                cart = db.Database.SqlQuery<CartItem>(
                    @"SELECT SanPhamId, TenSanPham, HinhAnh, DonGia, SoLuong
                      FROM GioHang WHERE Username = @p0 ORDER BY Id", username).ToList();
            }
            Session[CART_KEY] = cart;
            return cart;
        }

        // Lưu giỏ xuống SQL (gọi sau MỖI thay đổi)
        private void SaveCart(List<CartItem> cart)
        {
            Session[CART_KEY] = cart;
            string username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username)) return;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM GioHang WHERE Username = @p0", username);

                    foreach (var i in cart)
                    {
                        db.Database.ExecuteSqlCommand(
                            @"INSERT INTO GioHang (Username, SanPhamId, TenSanPham, HinhAnh, DonGia, SoLuong)
                              VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                            username, i.SanPhamId,
                            (object)i.TenSanPham ?? DBNull.Value,
                            (object)i.HinhAnh ?? DBNull.Value,
                            i.DonGia, i.SoLuong);
                    }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        private void ClearCart()
        {
            Session.Remove(CART_KEY);
            Session.Remove(VOUCHER_KEY);

            string username = Session["Username"] as string;
            if (!string.IsNullOrEmpty(username))
                db.Database.ExecuteSqlCommand(
                    "DELETE FROM GioHang WHERE Username = @p0", username);
        }

        // ============================================================
        //  VOUCHER
        // ============================================================

        private List<Voucher> LayDsVoucher()
        {
            var now = DateTime.Now;
            return db.Vouchers
                .Where(v => v.TrangThai && v.NgayBatDau <= now && v.NgayKetThuc >= now
                            && (v.SoLuong == null || v.DaDung < v.SoLuong))
                .OrderBy(v => v.NgayKetThuc)
                .ToList();
        }

        // Nạp dữ liệu voucher cho trang Checkout
        private void NapVoucherViewBag(List<CartItem> cart)
        {
            decimal tamTinh = cart.Sum(x => x.ThanhTien);
            ViewBag.DsVoucher = LayDsVoucher();
            ViewBag.VoucherMa = null;
            ViewBag.TienGiam = 0m;

            string ma = Session[VOUCHER_KEY] as string;
            if (!string.IsNullOrEmpty(ma))
            {
                var r = VoucherService.Kiem(db, ma, Session["Username"].ToString(), tamTinh);
                if (r.Ok)
                {
                    ViewBag.VoucherMa = r.Voucher.Ma;
                    ViewBag.TienGiam = r.TienGiam;
                }
                else
                {
                    Session.Remove(VOUCHER_KEY);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApDungVoucher(string ma)
        {
            if (Session["Username"] == null)
                return Json(new { success = false, requireLogin = true });

            decimal tamTinh = GetCart().Sum(x => x.ThanhTien);
            var r = VoucherService.Kiem(db, ma, Session["Username"].ToString(), tamTinh);

            if (!r.Ok)
            {
                Session.Remove(VOUCHER_KEY);
                return Json(new { success = false, message = r.Message, giam = 0m, tong = tamTinh });
            }

            Session[VOUCHER_KEY] = r.Voucher.Ma;
            return Json(new
            {
                success = true,
                message = r.Message,
                ma = r.Voucher.Ma,
                giam = r.TienGiam,
                tong = tamTinh - r.TienGiam
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GoVoucher()
        {
            Session.Remove(VOUCHER_KEY);
            decimal tamTinh = GetCart().Sum(x => x.ThanhTien);
            return Json(new { success = true, giam = 0m, tong = tamTinh });
        }

        // ============================================================
        //  GIO HANG
        // ============================================================

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

            SaveCart(cart);

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
            if (Session["Username"] == null)
            {
                return Json(new { success = false, requireLogin = true });
            }

            var cart = GetCart();

            cart.RemoveAll(x => x.SanPhamId == id);

            SaveCart(cart);

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
            if (Session["Username"] == null)
            {
                return Json(new { success = false, requireLogin = true });
            }

            if (soLuong < 0 || soLuong > 100)
            {
                return Json(new
                {
                    success = false,
                    message = "So luong khong hop le."
                });
            }

            var cart = GetCart();

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

            SaveCart(cart);

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
            ClearCart();

            return Json(new { success = true });
        }

        // ============================================================
        //  CHECKOUT + DAT HANG
        // ============================================================

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

            NapVoucherViewBag(cart);
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

                NapVoucherViewBag(cart);
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

            // ----- Tinh lai voucher o server (khong tin gia tri tu trinh duyet) -----
            string username = Session["Username"].ToString();
            decimal tamTinh = cart.Sum(x => x.ThanhTien);
            decimal tienGiam = 0;
            Voucher voucher = null;

            string maVoucher = Session[VOUCHER_KEY] as string;
            if (!string.IsNullOrEmpty(maVoucher))
            {
                var r = VoucherService.Kiem(db, maVoucher, username, tamTinh);
                if (!r.Ok)
                {
                    Session.Remove(VOUCHER_KEY);
                    ModelState.AddModelError("", "Voucher không còn hợp lệ: " + r.Message);
                    NapVoucherViewBag(cart);
                    return View("Checkout", cart);
                }

                voucher = r.Voucher;
                tienGiam = r.TienGiam;
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var donHang = new DonHang
                    {
                        Username = username,
                        HoTen = hoTen.Trim(),
                        SoDienThoai = soDienThoai.Trim(),
                        DiaChi = diaChi.Trim(),
                        GhiChu = ghiChu,
                        TongTien = tamTinh - tienGiam,
                        MaVoucher = voucher != null ? voucher.Ma : null,
                        TienGiam = tienGiam,
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

                    // GHI NHAN VOUCHER (tru luot an toan khi nhieu nguoi dung cung luc)
                    if (voucher != null)
                    {
                        int n = db.Database.ExecuteSqlCommand(
                            @"UPDATE Voucher SET DaDung = DaDung + 1
                              WHERE Id = @p0 AND (SoLuong IS NULL OR DaDung < SoLuong)",
                            voucher.Id);

                        if (n != 1)
                            throw new VoucherHetLuotException();

                        db.VoucherUsages.Add(new VoucherUsage
                        {
                            VoucherId = voucher.Id,
                            Username = username,
                            IdDonHang = donHang.Id,
                            NgaySuDung = DateTime.Now
                        });

                        db.SaveChanges();
                    }

                    int idDonHang = donHang.Id;

                    transaction.Commit();

                    // Đặt hàng xong: xóa giỏ (Session + SQL) và voucher đang chọn
                    ClearCart();

                    if (phuongThuc == "COD")
                    {
                        InvoiceMailService.GuiHoaDon(db, idDonHang);
                    }
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
                catch (VoucherHetLuotException)
                {
                    transaction.Rollback();

                    Session.Remove(VOUCHER_KEY);
                    ModelState.AddModelError("", "Voucher vừa hết lượt sử dụng. Vui lòng kiểm tra lại đơn hàng.");
                    NapVoucherViewBag(cart);
                    return View("Checkout", cart);
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