using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;
using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace Wedsite_LTWW.Controllers
{
    public class AccountController : Controller
    {
        accountEntities db = new accountEntities();
        public void SendOTP(string email, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Gas Spark", "tranbao78900@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Mã xác nhận đăng ký";

            message.Body = new TextPart("plain")
            {
                Text = "Mã OTP của bạn là: " + otp
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("tranbao78900@gmail.com", "tmvyburwncytypie");
                client.Send(message);
                client.Disconnect(true);
            }
        }
        // ===== ĐĂNG KÝ =====

        [HttpPost]
        public ActionResult VerifyOTP(string otp)
        {
            string otpSession = Session["OTP"] as string;

            if (otp == otpSession)
            {
                Users user = Session["UserRegister"] as Users;

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            ViewBag.Error = "OTP không đúng!";
            return View();
        }


        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(Users tk)
        {
            if (ModelState.IsValid)
            {
                var check = db.Users.FirstOrDefault(x => x.Username == tk.Username);

                if (check != null)
                {
                    ViewBag.Error = "Tài khoản đã tồn tại!";
                    return View(tk);
                }

                // tạo OTP
                Random rd = new Random();
                string otp = rd.Next(100000, 999999).ToString();

                // lưu OTP vào session
                Session["OTP"] = otp;

                // lưu thông tin user tạm
                Session["UserRegister"] = tk;

                // Gửi OTP vào Gmail
                SendOTP(tk.Email, otp);

                db.Users.Add(tk);
                db.SaveChanges();

                // chuyển sang trang đăng nhập
                return RedirectToAction("VerifyOTP", "Account");
            }

            return View(tk);
        }
        // ===== ĐĂNG NHẬP =====
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult VerifyOTP()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            var user = db.Users
                .FirstOrDefault(x => x.Username == Username && x.Password == Password);

            if (user != null)
            {
                Session["Username"] = user.Username;
                return RedirectToAction("Tranggiaodien", "Home");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        // ===== ĐĂNG XUẤT =====
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Tranggiaodien", "Home");
        }
    }
}