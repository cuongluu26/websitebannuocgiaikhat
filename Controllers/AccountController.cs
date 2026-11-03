using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class AccountController : Controller
    {
        GasSparkDBEntities db = new GasSparkDBEntities();
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
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOTP(string otp)
        {
            string otpSession = Session["OTP"] as string;

            if (otp == otpSession)
            {
                var username = Session["Username"] as string;
                var password = Session["Password"] as string;
                var email = Session["Email"] as string;

                if (username == null || password == null || email == null)
                {
                    ViewBag.Error = "Session bị mất!";
                    return View();
                }

                User user = new User()
                {
                    Username = username,
                    Password = password,
                    Email = email
                };

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            ViewBag.Error = "OTP không đúng!";
            return View();
        }


        [HttpPost]
        public ActionResult Register(User tk)
        {
            if (ModelState.IsValid)
            {
                var check = db.Users.FirstOrDefault(x => x.Email == tk.Email);


                if (check != null)
                {
                    ViewBag.Error = "Gmail đã tồn tại";
                    return View(tk);
                }

                string otp = new Random().Next(100000, 999999).ToString();

                Session["OTP"] = otp;
                Session["Username"] = tk.Username;
                Session["Password"] = tk.Password;
                Session["Email"] = tk.Email;

                SendOTP(tk.Email, otp);

                return RedirectToAction("VerifyOTP");
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
                Session["Role"] = user.Role;

                if (user.Role == "admin")
                    return RedirectToAction("Index", "Admin");
                else
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


        public ActionResult VerifyOTP1()
        {
            return View();          // POST: Xử lý đổi mật khẩu
        }

        [HttpPost]
        public ActionResult VerifyOTP1(string otp)
        {
            string sessionOtp = Session["OTP"] as string;

            if (sessionOtp == null)
            {
                ViewBag.Message = "OTP đã hết hạn!";
                return View();
            }

            if (otp != sessionOtp)
            {
                ViewBag.Message = "OTP không đúng!";
                return View();
            }

            return RedirectToAction("ResetPassword");
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            string fromEmail = "tranbao78900@gmail.com";
            string appPassword = "ziiq heav vkgx nhhs"; // App Password Gmail

            using (var smtp = new System.Net.Mail.SmtpClient())
            {
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                smtp.Send(fromEmail, toEmail, subject, body);
            }
        }

        public ActionResult ForgotPassword()
        {
            return View();          // POST: Xử lý đổi mật khẩu
        }
        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                ViewBag.Message = "Vui lòng nhập Email!";
                return View();
            }

            var user = db.Users.FirstOrDefault(x => x.Email == Email);

            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại!";
                return View();
            }

            // tạo OTP
            string otp = new Random().Next(100000, 999999).ToString();

            // lưu lại
            Session["OTP"] = otp;
            Session["Email"] = Email;
            Session["OTP_Type"] = "Forgot";

            // test (sau này gửi mail)
            SendOTP(Email, otp);

            // 👉 CHỖ QUAN TRỌNG
            return RedirectToAction("VerifyOTP1");
        }


        [HttpPost]
        public ActionResult ResetPassword(string newPassword)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            string email = Session["Email"].ToString();

            var user = db.Users.FirstOrDefault(x => x.Email == email);

            if (user != null)
            {
                user.Password = newPassword;   // (có thể nâng cấp hash sau)
                db.SaveChanges();
            }

            // xoá session sau khi đổi thành công
            Session.Remove("OTP");
            Session.Remove("Email");

            return RedirectToAction("Login", "Account");
        }

        public ActionResult ResetPassword()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

    }
}