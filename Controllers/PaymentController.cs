using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class SePayPayload
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("gateway")] public string Gateway { get; set; }
        [JsonProperty("transactionDate")] public string TransactionDate { get; set; }
        [JsonProperty("accountNumber")] public string AccountNumber { get; set; }
        [JsonProperty("code")] public string Code { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("transferType")] public string TransferType { get; set; }
        [JsonProperty("transferAmount")] public decimal TransferAmount { get; set; }
        [JsonProperty("referenceCode")] public string ReferenceCode { get; set; }
    }

    public class PaymentController : Controller
    {
        private static readonly string MY_ACCOUNT =
    ConfigurationManager.AppSettings["SePayAccount"];
        private readonly GasSparkDBEntities db = new GasSparkDBEntities();

        // SePay goi vao day khi co tien vao tai khoan
        [HttpPost]
        public ActionResult SePayWebhook()
        {
            // 1. Xac thuc API Key
            string expected = ConfigurationManager.AppSettings["SePayApiKey"];
            string auth = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(expected) || auth != "Apikey " + expected)
            {
                return new HttpStatusCodeResult(401);
            }

            // 2. Doc JSON
            SePayPayload p;
            try
            {
                Request.InputStream.Position = 0;
                string body = new StreamReader(Request.InputStream).ReadToEnd();
                p = JsonConvert.DeserializeObject<SePayPayload>(body);
            }
            catch
            {
                return new HttpStatusCodeResult(400);
            }

            if (p == null) return new HttpStatusCodeResult(400);

            // 3. Chi xu ly tien VAO, dung tai khoan cua minh
            if (p.TransferType != "in" || p.AccountNumber != MY_ACCOUNT)
            {
                return Json(new { success = true, message = "ignored" });
            }

            // 4. Tim ma don: uu tien truong code, khong co thi tim trong noi dung
            var m = Regex.Match(p.Code ?? "", @"GAS(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                m = Regex.Match(p.Content ?? "", @"GAS(\d+)", RegexOptions.IgnoreCase);
            }

            if (!m.Success)
            {
                return Json(new { success = true, message = "no order code" });
            }

            int idDonHang;
            if (!int.TryParse(m.Groups[1].Value, out idDonHang))
            {
                return Json(new { success = true, message = "bad order code" });
            }

            var donHang = db.DonHangs.FirstOrDefault(x =>
                x.Id == idDonHang &&
                x.PhuongThucThanhToan == "Banking");

            if (donHang == null)
            {
                return Json(new { success = true, message = "order not found" });
            }

            // 5. Da thanh toan roi thi bo qua (SePay co the goi lai)
            if (donHang.TrangThaiThanhToan == "Đã thanh toán")
            {
                return Json(new { success = true, message = "already paid" });
            }

            // 6. Kiem tra du tien
            if (p.TransferAmount < donHang.TongTien)
            {
                return Json(new { success = true, message = "amount not enough" });
            }

            donHang.TrangThaiThanhToan = "Đã thanh toán";
            db.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}