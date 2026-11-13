using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Hosting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

// Dat file nay vao thu muc Models (cung cho voi VoucherService)
namespace Wedsite_LTWW.Models
{
    public class InvoiceLine
    {
        public string Ten { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get { return DonGia * SoLuong; } }
    }

    public class InvoiceData
    {
        public int Id { get; set; }
        public string MaDon { get { return "GAS" + Id; } }
        public string Email { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayDat { get; set; }
        public string PhuongThuc { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public string MaVoucher { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongTien { get; set; }
        public decimal TamTinh { get; set; }
        public List<InvoiceLine> Lines { get; set; }
    }

    public static class InvoiceMailService
    {
        private static readonly CultureInfo VN = new CultureInfo("vi-VN");

        // ============================================================
        //  HAM GOI TU CONTROLLER
        //  Doc du lieu NGAY trong request (db con song), con viec tao PDF + gui mail
        //  chay nen de khong lam cham khach.
        // ============================================================
        public static void GuiHoaDon(GasSparkDBEntities db, int idDonHang)
        {
            try
            {
                var dh = db.DonHangs.FirstOrDefault(x => x.Id == idDonHang);
                if (dh == null) return;

                // Email lay tu bang User theo Username
                string email = db.Users
                    .Where(u => u.Username == dh.Username)
                    .Select(u => u.Email)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(email)) { Log("Don " + idDonHang + ": khach khong co email."); return; }

                var lines = db.ChiTietDonHangs
                    .Where(c => c.IdDonHang == idDonHang)
                    .ToList()
                    .Select(c => new InvoiceLine
                    {
                        Ten = c.TenSanPham,
                        DonGia = Convert.ToDecimal(c.DonGia),
                        SoLuong = Convert.ToInt32(c.SoLuong)
                    }).ToList();

                var data = new InvoiceData
                {
                    Id = dh.Id,
                    Email = email.Trim(),
                    HoTen = dh.HoTen,
                    SoDienThoai = dh.SoDienThoai,
                    DiaChi = dh.DiaChi,
                    GhiChu = dh.GhiChu,
                    NgayDat = Convert.ToDateTime(dh.NgayDat),
                    PhuongThuc = dh.PhuongThucThanhToan,
                    TrangThaiThanhToan = dh.TrangThaiThanhToan,
                    MaVoucher = dh.MaVoucher,
                    TienGiam = Convert.ToDecimal(dh.TienGiam),
                    TongTien = Convert.ToDecimal(dh.TongTien),
                    Lines = lines
                };
                data.TamTinh = lines.Sum(x => x.ThanhTien);

                HostingEnvironment.QueueBackgroundWorkItem(ct => Send(data));
            }
            catch (Exception ex)
            {
                Log("GuiHoaDon loi: " + ex);
            }
        }

        // ============================================================
        //  GUI MAIL
        // ============================================================
        private static void Send(InvoiceData d)
        {
            try
            {
                string host = Cfg("Smtp:Host", "smtp.gmail.com");
                int port = int.Parse(Cfg("Smtp:Port", "587"));
                string user = Cfg("Smtp:User", "");
                string pass = Cfg("Smtp:Pass", "");
                string shop = Cfg("Shop:Name", "GasSpark");

                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                byte[] pdf = BuildPdf(d);

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(user, shop, Encoding.UTF8);
                    msg.To.Add(d.Email);
                    msg.Subject = "[" + shop + "] Hóa đơn đơn hàng " + d.MaDon;
                    msg.SubjectEncoding = Encoding.UTF8;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Body = BuildHtml(d);
                    msg.Attachments.Add(new Attachment(
                        new MemoryStream(pdf), "HoaDon_" + d.MaDon + ".pdf", "application/pdf"));

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = true;
                        smtp.Credentials = new NetworkCredential(user, pass);
                        smtp.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Gui mail don " + d.MaDon + " loi: " + ex);
            }
        }

        // ============================================================
        //  NOI DUNG EMAIL (HTML)
        // ============================================================
        private static string BuildHtml(InvoiceData d)
        {
            Func<string, string> e = s => WebUtility.HtmlEncode(s ?? "");
            string shop = Cfg("Shop:Name", "GasSpark");

            var sb = new StringBuilder();
            sb.Append("<div style='font-family:Arial,Helvetica,sans-serif;max-width:680px;margin:auto;color:#222;'>");
            sb.Append("<div style='background:#1f6feb;color:#fff;padding:18px 22px;'>");
            sb.Append("<div style='font-size:22px;font-weight:bold;'>" + e(shop) + "</div>");
            sb.Append("<div style='font-size:13px;'>" + e(Cfg("Shop:Address", "")) + "</div>");
            sb.Append("<div style='font-size:13px;'>ĐT: " + e(Cfg("Shop:Phone", "")) +
                      " | Email: " + e(Cfg("Shop:Email", "")) +
                      (string.IsNullOrEmpty(Cfg("Shop:TaxCode", "")) ? "" : " | MST: " + e(Cfg("Shop:TaxCode", ""))) + "</div>");
            sb.Append("</div>");

            sb.Append("<div style='padding:18px 22px;border:1px solid #ddd;border-top:0;'>");
            sb.Append("<h2 style='margin:0 0 6px;'>HÓA ĐƠN BÁN HÀNG</h2>");
            sb.Append("<p>Xin chào <b>" + e(d.HoTen) + "</b>, cảm ơn bạn đã đặt hàng tại " + e(shop) + ". Thông tin đơn hàng của bạn:</p>");

            sb.Append("<table style='width:100%;font-size:14px;margin-bottom:12px;'>");
            Row2(sb, "Mã đơn hàng", "<b>" + e(d.MaDon) + "</b>");
            Row2(sb, "Ngày đặt", d.NgayDat.ToString("dd/MM/yyyy HH:mm"));
            Row2(sb, "Người nhận", e(d.HoTen));
            Row2(sb, "Số điện thoại", e(d.SoDienThoai));
            Row2(sb, "Địa chỉ giao hàng", e(d.DiaChi));
            if (!string.IsNullOrWhiteSpace(d.GhiChu)) Row2(sb, "Ghi chú", e(d.GhiChu));
            Row2(sb, "Thanh toán", e(PhuongThucText(d.PhuongThuc)));
            Row2(sb, "Trạng thái thanh toán", e(d.TrangThaiThanhToan));
            sb.Append("</table>");

            sb.Append("<table style='width:100%;border-collapse:collapse;font-size:14px;'>");
            sb.Append("<tr style='background:#f0f3f8;'>" +
                      Th("STT", "center") + Th("Sản phẩm", "left") + Th("SL", "center") +
                      Th("Đơn giá", "right") + Th("Thành tiền", "right") + "</tr>");
            int i = 1;
            foreach (var l in d.Lines)
            {
                sb.Append("<tr>" + Td(i++.ToString(), "center") + Td(e(l.Ten), "left") +
                          Td(l.SoLuong.ToString(), "center") + Td(Money(l.DonGia), "right") +
                          Td(Money(l.ThanhTien), "right") + "</tr>");
            }
            sb.Append("</table>");

            sb.Append("<table style='width:100%;font-size:14px;margin-top:10px;'>");
            TotalRow(sb, "Tạm tính", Money(d.TamTinh), false);
            if (d.TienGiam > 0)
                TotalRow(sb, "Giảm giá" + (string.IsNullOrEmpty(d.MaVoucher) ? "" : " (" + e(d.MaVoucher) + ")"),
                         "-" + Money(d.TienGiam), false);
            TotalRow(sb, "TỔNG THANH TOÁN", Money(d.TongTien), true);
            sb.Append("</table>");

            if (d.PhuongThuc == "Banking" && d.TrangThaiThanhToan == "Đã thanh toán")
                sb.Append("<p style='color:#0a7d2c;'>✔ Chúng tôi đã nhận được thanh toán của bạn.</p>");
            else if (d.PhuongThuc == "COD")
                sb.Append("<p>Bạn vui lòng chuẩn bị <b>" + Money(d.TongTien) + "</b> để thanh toán khi nhận hàng.</p>");

            sb.Append("<p style='font-size:13px;color:#666;'>Hóa đơn dạng PDF được đính kèm trong email này. " +
                      "Nếu cần hỗ trợ, vui lòng liên hệ " + e(Cfg("Shop:Phone", "")) + ".</p>");
            sb.Append("</div></div>");
            return sb.ToString();
        }

        private static void Row2(StringBuilder sb, string k, string v)
        {
            sb.Append("<tr><td style='width:170px;color:#666;padding:3px 0;vertical-align:top;'>" + k +
                      "</td><td style='padding:3px 0;'>" + v + "</td></tr>");
        }
        private static string Th(string t, string a)
        {
            return "<th style='padding:8px;border:1px solid #ddd;text-align:" + a + ";'>" + t + "</th>";
        }
        private static string Td(string t, string a)
        {
            return "<td style='padding:8px;border:1px solid #ddd;text-align:" + a + ";'>" + t + "</td>";
        }
        private static void TotalRow(StringBuilder sb, string k, string v, bool bold)
        {
            string st = bold ? "font-weight:bold;font-size:16px;color:#c00;" : "";
            sb.Append("<tr><td style='text-align:right;padding:3px 0;" + st + "'>" + k +
                      ":</td><td style='text-align:right;width:160px;padding:3px 0;" + st + "'>" + v + "</td></tr>");
        }

        // ============================================================
        //  FILE PDF (PDFsharp-MigraDoc)
        // ============================================================
        private static byte[] BuildPdf(InvoiceData d)
        {
            var doc = new Document();
            doc.Styles["Normal"].Font.Name = "Arial";
            doc.Styles["Normal"].Font.Size = 10;

            var sec = doc.AddSection();
            sec.PageSetup.PageFormat = PageFormat.A4;
            sec.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            sec.PageSetup.RightMargin = Unit.FromCentimeter(2);
            sec.PageSetup.TopMargin = Unit.FromCentimeter(1.8);

            var p = sec.AddParagraph(Cfg("Shop:Name", "GasSpark"));
            p.Format.Font.Size = 18; p.Format.Font.Bold = true;
            p.Format.Font.Color = new Color(31, 111, 235);
            sec.AddParagraph(Cfg("Shop:Address", ""));
            sec.AddParagraph("ĐT: " + Cfg("Shop:Phone", "") + "   Email: " + Cfg("Shop:Email", ""));
            if (!string.IsNullOrEmpty(Cfg("Shop:TaxCode", "")))
                sec.AddParagraph("MST: " + Cfg("Shop:TaxCode", ""));

            p = sec.AddParagraph("HÓA ĐƠN BÁN HÀNG");
            p.Format.Font.Size = 16; p.Format.Font.Bold = true;
            p.Format.Alignment = ParagraphAlignment.Center;
            p.Format.SpaceBefore = Unit.FromCentimeter(0.6);
            p.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            Info(sec, "Mã đơn hàng", d.MaDon);
            Info(sec, "Ngày đặt", d.NgayDat.ToString("dd/MM/yyyy HH:mm"));
            Info(sec, "Người nhận", d.HoTen);
            Info(sec, "Số điện thoại", d.SoDienThoai);
            Info(sec, "Địa chỉ", d.DiaChi);
            if (!string.IsNullOrWhiteSpace(d.GhiChu)) Info(sec, "Ghi chú", d.GhiChu);
            Info(sec, "Thanh toán", PhuongThucText(d.PhuongThuc));
            Info(sec, "Trạng thái TT", d.TrangThaiThanhToan);

            var t = sec.AddTable();
            t.Style = "Normal";
            t.Borders.Width = 0.5;
            t.Borders.Color = Colors.Gray;
            t.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            t.TopPadding = 3; t.BottomPadding = 3;
            t.AddColumn(Unit.FromCentimeter(1.1));
            t.AddColumn(Unit.FromCentimeter(6.9));
            t.AddColumn(Unit.FromCentimeter(1.3));
            t.AddColumn(Unit.FromCentimeter(3.4));
            t.AddColumn(Unit.FromCentimeter(3.4));

            var h = t.AddRow();
            h.Shading.Color = Colors.LightGray;
            h.Format.Font.Bold = true;
            string[] heads = { "STT", "Sản phẩm", "SL", "Đơn giá", "Thành tiền" };
            for (int c = 0; c < heads.Length; c++) h.Cells[c].AddParagraph(heads[c]);
            h.Cells[0].Format.Alignment = ParagraphAlignment.Center;
            h.Cells[2].Format.Alignment = ParagraphAlignment.Center;
            h.Cells[3].Format.Alignment = ParagraphAlignment.Right;
            h.Cells[4].Format.Alignment = ParagraphAlignment.Right;

            int i = 1;
            foreach (var l in d.Lines)
            {
                var r = t.AddRow();
                r.Cells[0].AddParagraph((i++).ToString());
                r.Cells[1].AddParagraph(l.Ten ?? "");
                r.Cells[2].AddParagraph(l.SoLuong.ToString());
                r.Cells[3].AddParagraph(Money(l.DonGia));
                r.Cells[4].AddParagraph(Money(l.ThanhTien));
                r.Cells[0].Format.Alignment = ParagraphAlignment.Center;
                r.Cells[2].Format.Alignment = ParagraphAlignment.Center;
                r.Cells[3].Format.Alignment = ParagraphAlignment.Right;
                r.Cells[4].Format.Alignment = ParagraphAlignment.Right;
            }

            Total(sec, "Tạm tính: " + Money(d.TamTinh), false, 0.4);
            if (d.TienGiam > 0)
                Total(sec, "Giảm giá" + (string.IsNullOrEmpty(d.MaVoucher) ? "" : " (" + d.MaVoucher + ")") +
                           ": -" + Money(d.TienGiam), false, 0);
            Total(sec, "TỔNG THANH TOÁN: " + Money(d.TongTien), true, 0.1);

            p = sec.AddParagraph("Cảm ơn quý khách đã mua hàng!");
            p.Format.Alignment = ParagraphAlignment.Center;
            p.Format.Font.Italic = true;
            p.Format.SpaceBefore = Unit.FromCentimeter(1);

            var renderer = new PdfDocumentRenderer(true) { Document = doc };
            renderer.RenderDocument();
            using (var ms = new MemoryStream())
            {
                renderer.PdfDocument.Save(ms, false);
                return ms.ToArray();
            }
        }

        private static void Info(Section sec, string k, string v)
        {
            var p = sec.AddParagraph();
            p.AddFormattedText(k + ": ", TextFormat.Bold);
            p.AddText(v ?? "");
        }

        private static void Total(Section sec, string text, bool big, double spaceBeforeCm)
        {
            var p = sec.AddParagraph(text);
            p.Format.Alignment = ParagraphAlignment.Right;
            p.Format.SpaceBefore = Unit.FromCentimeter(spaceBeforeCm);
            if (big)
            {
                p.Format.Font.Bold = true;
                p.Format.Font.Size = 13;
                p.Format.Font.Color = new Color(200, 0, 0);
            }
        }

        // ============================================================
        //  TIEN ICH
        // ============================================================
        private static string Money(decimal v)
        {
            return string.Format(VN, "{0:N0} đ", v);
        }

        private static string PhuongThucText(string pt)
        {
            return pt == "Banking" ? "Chuyển khoản ngân hàng" : "Thanh toán khi nhận hàng (COD)";
        }

        private static string Cfg(string key, string def)
        {
            var v = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(v) ? def : v;
        }

        private static void Log(string msg)
        {
            try
            {
                string dir = HostingEnvironment.MapPath("~/App_Data");
                File.AppendAllText(Path.Combine(dir, "mail-error.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + msg + Environment.NewLine);
            }
            catch { }
        }
    }
}