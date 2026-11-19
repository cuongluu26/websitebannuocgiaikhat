using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;

namespace Wedsite_LTWW.Controllers
{
    public class DashboardVM
    {
        // Bo loc thoi gian
        public string Range { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // The dem tong (giu lai nhu cu)
        public int TongSanPham { get; set; }
        public int TongDanhMuc { get; set; }
        public int TongDonHang { get; set; }
        public int TongUser { get; set; }
        public int TongLienHe { get; set; }
        public int LienHeChuaDoc { get; set; }
        public int TongVoucher { get; set; }

        // Doanh thu nhanh
        public decimal DoanhThuHomNay { get; set; }
        public decimal? HomNayPct { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal? ThangPct { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int ChoXacNhan { get; set; }
        public int ChoThanhToan { get; set; }

        // Theo ky dang chon
        public decimal DoanhThuKy { get; set; }
        public decimal? DoanhThuKyPct { get; set; }
        public int DonKy { get; set; }
        public decimal? DonKyPct { get; set; }
        public decimal GiaTriTB { get; set; }
        public double TyLeHuy { get; set; }

        // Du lieu bieu do
        public List<string> Labels { get; set; }
        public List<decimal> DoanhThuData { get; set; }
        public List<int> DonData { get; set; }
        public List<string> TrangThaiLabels { get; set; }
        public List<int> TrangThaiData { get; set; }
        public List<string> DanhMucLabels { get; set; }
        public List<decimal> DanhMucData { get; set; }

        // Bang
        public List<TopSpVM> TopSanPham { get; set; }
        public List<RecentOrderVM> DonGanDay { get; set; }
        public List<VoucherAlertVM> VoucherCanhBao { get; set; }
    }

    public class TopSpVM
    {
        public string Ten { get; set; }
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class RecentOrderVM
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public decimal TongTien { get; set; }
        public DateTime? NgayDat { get; set; }
        public string TrangThai { get; set; }
        public string PhuongThuc { get; set; }
    }

    public class VoucherAlertVM
    {
        public string Ma { get; set; }
        public string TieuDe { get; set; }
        public string GhiChu { get; set; }
    }

    // =========================================================
    // Phan dashboard cua AdminController
    // =========================================================
    public partial class AdminController
    {
        private const string HOAN_THANH = "Hoàn thành";
        private const string DA_HUY = "Đã hủy";
        private const string CHO_XAC_NHAN = "Chờ xác nhận";

        // Doanh thu = tong tien cac don "Hoan thanh" trong [from, toExcl)
        private decimal SumRevenue(DateTime from, DateTime toExcl)
        {
            return db.DonHangs
                .Where(x => x.TrangThai == HOAN_THANH
                         && x.NgayDat >= from && x.NgayDat < toExcl)
                .Select(x => (decimal?)x.TongTien)
                .Sum() ?? 0;
        }

        private static decimal? PctChange(decimal cur, decimal prev)
        {
            if (prev == 0) return cur == 0 ? 0 : (decimal?)null;
            return Math.Round((cur - prev) / prev * 100, 1);
        }

        public ActionResult Index(string range, DateTime? tuNgay, DateTime? denNgay)
        {
            var today = DateTime.Today;
            DateTime start, end;

            if (tuNgay.HasValue && denNgay.HasValue)
            {
                start = tuNgay.Value.Date;
                end = denNgay.Value.Date;
                if (end < start) { var t = start; start = end; end = t; }
                if ((end - start).Days > 730) start = end.AddDays(-730);
                range = "custom";
            }
            else
            {
                int days;
                switch (range)
                {
                    case "7": days = 7; break;
                    case "90": days = 90; break;
                    case "365": days = 365; break;
                    default: days = 30; range = "30"; break;
                }
                end = today;
                start = today.AddDays(-(days - 1));
            }

            var endExcl = end.AddDays(1);
            var length = (end - start).Days + 1;
            var prevStart = start.AddDays(-length);

            var vm = new DashboardVM { Range = range, Start = start, End = end };

            // ---- The dem tong ----
            vm.TongSanPham = db.SanPhams.Count();
            vm.TongDanhMuc = db.DanhMucs.Count();
            vm.TongDonHang = db.DonHangs.Count();
            vm.TongUser = db.Users.Count();
            vm.TongLienHe = db.LienHes.Count();
            vm.LienHeChuaDoc = db.LienHes.Count(x => x.DaDoc == false);
            vm.TongVoucher = db.Vouchers.Count();

            // ---- Doanh thu nhanh ----
            vm.DoanhThuHomNay = SumRevenue(today, today.AddDays(1));
            vm.HomNayPct = PctChange(vm.DoanhThuHomNay,
                SumRevenue(today.AddDays(-1), today));

            var dauThang = new DateTime(today.Year, today.Month, 1);
            var dauThangTruoc = dauThang.AddMonths(-1);
            var cungKy = dauThangTruoc.AddDays(today.Day);
            if (cungKy > dauThang) cungKy = dauThang;
            vm.DoanhThuThang = SumRevenue(dauThang, today.AddDays(1));
            vm.ThangPct = PctChange(vm.DoanhThuThang,
                SumRevenue(dauThangTruoc, cungKy));

            vm.TongDoanhThu = db.DonHangs
                .Where(x => x.TrangThai == HOAN_THANH)
                .Select(x => (decimal?)x.TongTien).Sum() ?? 0;

            vm.ChoXacNhan = db.DonHangs.Count(x => x.TrangThai == CHO_XAC_NHAN);
            vm.ChoThanhToan = db.DonHangs.Count(x =>
                x.PhuongThucThanhToan == "Banking" &&
                x.TrangThaiThanhToan != "Đã thanh toán");

            // ---- Don hang trong ky ----
            var orders = db.DonHangs
                .Where(x => x.NgayDat >= start && x.NgayDat < endExcl)
                .Select(x => new
                {
                    Tong = (decimal?)x.TongTien,
                    Ngay = (DateTime?)x.NgayDat,
                    x.TrangThai
                }).ToList();

            var hoanThanh = orders.Where(o => o.TrangThai == HOAN_THANH).ToList();
            vm.DoanhThuKy = hoanThanh.Sum(o => o.Tong ?? 0);
            vm.DoanhThuKyPct = PctChange(vm.DoanhThuKy, SumRevenue(prevStart, start));
            vm.DonKy = orders.Count;
            var donKyTruoc = db.DonHangs.Count(x =>
                x.NgayDat >= prevStart && x.NgayDat < start);
            vm.DonKyPct = PctChange(vm.DonKy, donKyTruoc);
            vm.GiaTriTB = hoanThanh.Count > 0 ? vm.DoanhThuKy / hoanThanh.Count : 0;
            vm.TyLeHuy = orders.Count > 0
                ? Math.Round(orders.Count(o => o.TrangThai == DA_HUY) * 100.0 / orders.Count, 1)
                : 0;

            // ---- Bieu do doanh thu: theo ngay, hoac theo thang neu ky > 92 ngay ----
            var inv = CultureInfo.InvariantCulture;
            vm.Labels = new List<string>();
            vm.DoanhThuData = new List<decimal>();
            vm.DonData = new List<int>();

            if (length <= 92)
            {
                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    var day = d;
                    vm.Labels.Add(day.ToString("dd/MM", inv));
                    vm.DoanhThuData.Add(hoanThanh
                        .Where(o => o.Ngay.HasValue && o.Ngay.Value.Date == day)
                        .Sum(o => o.Tong ?? 0));
                    vm.DonData.Add(orders
                        .Count(o => o.Ngay.HasValue && o.Ngay.Value.Date == day));
                }
            }
            else
            {
                for (var m = new DateTime(start.Year, start.Month, 1); m <= end; m = m.AddMonths(1))
                {
                    var mm = m;
                    vm.Labels.Add(mm.ToString("MM/yyyy", inv));
                    vm.DoanhThuData.Add(hoanThanh
                        .Where(o => o.Ngay.HasValue && o.Ngay.Value.Year == mm.Year
                                 && o.Ngay.Value.Month == mm.Month)
                        .Sum(o => o.Tong ?? 0));
                    vm.DonData.Add(orders
                        .Count(o => o.Ngay.HasValue && o.Ngay.Value.Year == mm.Year
                                 && o.Ngay.Value.Month == mm.Month));
                }
            }

            // ---- Don theo trang thai ----
            vm.TrangThaiLabels = DS_TRANG_THAI_DON.ToList();
            vm.TrangThaiData = DS_TRANG_THAI_DON
                .Select(s => orders.Count(o => o.TrangThai == s)).ToList();

            // ---- Chi tiet don hoan thanh trong ky: top SP + doanh thu theo danh muc ----
            var rows = (from ct in db.ChiTietDonHangs
                        join dh in db.DonHangs on ct.IdDonHang equals dh.Id
                        where dh.TrangThai == HOAN_THANH
                           && dh.NgayDat >= start && dh.NgayDat < endExcl
                        select new
                        {
                            IdSp = (int?)ct.IdSanPham,
                            Ten = ct.TenSanPham,
                            Gia = (decimal?)ct.DonGia,
                            SL = (int?)ct.SoLuong
                        }).ToList();

            vm.TopSanPham = rows
                .GroupBy(r => r.IdSp)
                .Select(g => new TopSpVM
                {
                    Ten = g.Select(x => x.Ten).FirstOrDefault(),
                    SoLuong = g.Sum(x => x.SL ?? 0),
                    DoanhThu = g.Sum(x => (x.Gia ?? 0) * (x.SL ?? 0))
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenByDescending(x => x.DoanhThu)
                .Take(5).ToList();

            var spDm = db.SanPhams
                .Select(x => new { x.Id, Dm = (int?)x.IdDanhMuc })
                .ToList().ToDictionary(x => x.Id, x => x.Dm);
            var tenDm = db.DanhMucs
                .Select(x => new { x.Id, x.TenDanhMuc })
                .ToList().ToDictionary(x => x.Id, x => x.TenDanhMuc);

            var theoDm = rows
                .GroupBy(r =>
                {
                    int? dm = null;
                    if (r.IdSp.HasValue && spDm.ContainsKey(r.IdSp.Value))
                        dm = spDm[r.IdSp.Value];
                    return (dm.HasValue && tenDm.ContainsKey(dm.Value))
                        ? tenDm[dm.Value] : "Khác";
                })
                .Select(g => new
                {
                    Ten = g.Key,
                    Tien = g.Sum(x => (x.Gia ?? 0) * (x.SL ?? 0))
                })
                .OrderByDescending(x => x.Tien).ToList();

            vm.DanhMucLabels = theoDm.Select(x => x.Ten).ToList();
            vm.DanhMucData = theoDm.Select(x => x.Tien).ToList();

            // ---- 8 don gan day ----
            vm.DonGanDay = db.DonHangs
                .OrderByDescending(x => x.NgayDat).ThenByDescending(x => x.Id)
                .Take(8)
                .Select(x => new
                {
                    x.Id,
                    x.HoTen,
                    Tong = (decimal?)x.TongTien,
                    Ngay = (DateTime?)x.NgayDat,
                    x.TrangThai,
                    x.PhuongThucThanhToan
                }).ToList()
                .Select(x => new RecentOrderVM
                {
                    Id = x.Id,
                    HoTen = x.HoTen,
                    TongTien = x.Tong ?? 0,
                    NgayDat = x.Ngay,
                    TrangThai = x.TrangThai,
                    PhuongThuc = x.PhuongThucThanhToan
                }).ToList();

            // ---- Voucher sap het han / sap het luot ----
            var vouchers = db.Vouchers.Select(x => new
            {
                x.Ma,
                x.TieuDe,
                Het = (DateTime?)x.NgayKetThuc,
                SL = (int?)x.SoLuong,
                Dung = (int?)x.DaDung
            }).ToList();

            vm.VoucherCanhBao = new List<VoucherAlertVM>();
            foreach (var v in vouchers)
            {
                var notes = new List<string>();
                var daysLeft = int.MaxValue;
                if (v.Het.HasValue)
                {
                    daysLeft = (v.Het.Value.Date - today).Days;
                    if (daysLeft < 0) continue; // da het han, bo qua
                    if (daysLeft == 0) notes.Add("Hết hạn hôm nay");
                    else if (daysLeft <= 7) notes.Add("Hết hạn sau " + daysLeft + " ngày");
                }
                if (v.SL.HasValue && v.SL.Value > 0)
                {
                    var conLai = v.SL.Value - (v.Dung ?? 0);
                    if (conLai <= 0) notes.Add("Hết lượt");
                    else if (conLai <= 5) notes.Add("Còn " + conLai + " lượt");
                }
                if (notes.Count > 0)
                {
                    vm.VoucherCanhBao.Add(new VoucherAlertVM
                    {
                        Ma = v.Ma,
                        TieuDe = v.TieuDe,
                        GhiChu = string.Join(", ", notes)
                    });
                }
            }

            return View(vm);
        }

        // =====================================================
        // XUAT BAO CAO EXCEL (.xlsx) - dung ClosedXML
        // =====================================================
        public ActionResult ExportDoanhThu(DateTime? tuNgay, DateTime? denNgay)
        {
            var start = (tuNgay ?? DateTime.Today.AddDays(-29)).Date;
            var end = (denNgay ?? DateTime.Today).Date;
            if (end < start) { var t = start; start = end; end = t; }
            if ((end - start).Days > 730) start = end.AddDays(-730);
            var endExcl = end.AddDays(1);
            var inv = CultureInfo.InvariantCulture;

            var data = db.DonHangs
                .Where(x => x.NgayDat >= start && x.NgayDat < endExcl)
                .OrderBy(x => x.NgayDat).ThenBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.HoTen,
                    x.SoDienThoai,
                    Ngay = (DateTime?)x.NgayDat,
                    x.TrangThai,
                    x.PhuongThucThanhToan,
                    x.TrangThaiThanhToan,
                    Tong = (decimal?)x.TongTien
                }).ToList();

            using (var wb = new XLWorkbook())
            {
                // ---------- Sheet 1: danh sach don ----------
                var ws = wb.Worksheets.Add("Đơn hàng");
                ws.Cell(1, 1).Value = "BÁO CÁO DOANH THU - GAS SPARK";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(2, 1).Value = "Từ ngày " + start.ToString("dd/MM/yyyy", inv)
                                    + " đến ngày " + end.ToString("dd/MM/yyyy", inv);

                var heads = new[] { "Mã đơn", "Khách hàng", "SĐT", "Ngày đặt",
                                    "Trạng thái", "Thanh toán", "TT thanh toán", "Tổng tiền (đ)" };
                const int hr = 4;
                for (int i = 0; i < heads.Length; i++)
                    ws.Cell(hr, i + 1).Value = heads[i];

                var hdr = ws.Range(hr, 1, hr, heads.Length);
                hdr.Style.Font.Bold = true;
                hdr.Style.Font.FontColor = XLColor.White;
                hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");

                int row = hr + 1;
                foreach (var r in data)
                {
                    ws.Cell(row, 1).Value = r.Id;
                    ws.Cell(row, 2).Value = r.HoTen ?? "";

                    // SDT luu dang text de giu so 0 dau
                    ws.Cell(row, 3).Style.NumberFormat.Format = "@";
                    ws.Cell(row, 3).SetValue(r.SoDienThoai ?? "");

                    if (r.Ngay.HasValue)
                    {
                        ws.Cell(row, 4).Value = r.Ngay.Value.Date;
                        ws.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                    }

                    ws.Cell(row, 5).Value = r.TrangThai ?? "";
                    ws.Cell(row, 6).Value = r.PhuongThucThanhToan ?? "";
                    ws.Cell(row, 7).Value = r.TrangThaiThanhToan ?? "";
                    ws.Cell(row, 8).Value = r.Tong ?? 0;
                    ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                    row++;
                }

                var tongHoanThanh = data.Where(r => r.TrangThai == HOAN_THANH)
                                        .Sum(r => r.Tong ?? 0);
                ws.Cell(row, 7).Value = "Doanh thu (đơn Hoàn thành)";
                ws.Cell(row, 8).Value = tongHoanThanh;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                ws.Range(row, 7, row, 8).Style.Font.Bold = true;

                ws.SheetView.FreezeRows(hr);
                ws.Columns().AdjustToContents();

                // ---------- Sheet 2: doanh thu theo ngay ----------
                var ws2 = wb.Worksheets.Add("Theo ngày");
                var h2 = new[] { "Ngày", "Số đơn đặt", "Số đơn hoàn thành", "Doanh thu (đ)" };
                for (int i = 0; i < h2.Length; i++)
                    ws2.Cell(1, i + 1).Value = h2[i];
                var hdr2 = ws2.Range(1, 1, 1, h2.Length);
                hdr2.Style.Font.Bold = true;
                hdr2.Style.Font.FontColor = XLColor.White;
                hdr2.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");

                int r2 = 2;
                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    var day = d;
                    var trongNgay = data.Where(x => x.Ngay.HasValue && x.Ngay.Value.Date == day).ToList();
                    var xong = trongNgay.Where(x => x.TrangThai == HOAN_THANH).ToList();

                    ws2.Cell(r2, 1).Value = day;
                    ws2.Cell(r2, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws2.Cell(r2, 2).Value = trongNgay.Count;
                    ws2.Cell(r2, 3).Value = xong.Count;
                    ws2.Cell(r2, 4).Value = xong.Sum(x => x.Tong ?? 0);
                    ws2.Cell(r2, 4).Style.NumberFormat.Format = "#,##0";
                    r2++;
                }
                ws2.SheetView.FreezeRows(1);
                ws2.Columns().AdjustToContents();

                var fileName = "DoanhThu_" + start.ToString("yyyyMMdd", inv)
                             + "_" + end.ToString("yyyyMMdd", inv) + ".xlsx";

                using (var ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    return File(ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }
    }
}