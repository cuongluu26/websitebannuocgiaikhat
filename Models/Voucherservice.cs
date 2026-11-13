using System;
using System.Globalization;
using System.Linq;

namespace Wedsite_LTWW.Models
{
    public class VoucherResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; }
        public Voucher Voucher { get; set; }
        public decimal TienGiam { get; set; }
    }

    public static class VoucherService
    {
        public static decimal TinhGiam(Voucher v, decimal tamTinh)
        {
            decimal giam = v.LoaiGiam == "PhanTram"
                ? Math.Round(tamTinh * v.GiaTri / 100m, 0)
                : v.GiaTri;

            if (v.GiamToiDa.HasValue && v.GiamToiDa.Value > 0 && giam > v.GiamToiDa.Value)
                giam = v.GiamToiDa.Value;

            if (giam > tamTinh) giam = tamTinh;
            if (giam < 0) giam = 0;
            return giam;
        }

        // Kiem tra voucher o phia server (dung ca luc ap dung va luc dat hang)
        public static VoucherResult Kiem(GasSparkDBEntities db, string ma, string username, decimal tamTinh)
        {
            ma = (ma ?? "").Trim();
            if (ma.Length == 0)
                return Loi("Vui lòng nhập mã giảm giá.");

            var v = db.Vouchers.FirstOrDefault(x => x.Ma == ma);
            if (v == null) return Loi("Mã giảm giá không tồn tại.");

            var now = DateTime.Now;
            if (!v.TrangThai) return Loi("Mã giảm giá đã ngừng áp dụng.");
            if (now < v.NgayBatDau) return Loi("Mã giảm giá chưa đến thời gian sử dụng.");
            if (now > v.NgayKetThuc) return Loi("Mã giảm giá đã hết hạn.");
            if (v.SoLuong.HasValue && v.DaDung >= v.SoLuong.Value) return Loi("Mã giảm giá đã hết lượt sử dụng.");

            if (tamTinh < v.DonToiThieu)
                return Loi("Đơn hàng tối thiểu " + v.DonToiThieu.ToString("N0", CultureInfo.InvariantCulture) + " VNĐ để dùng mã này.");

            int daDungCuaToi = db.VoucherUsages.Count(u => u.VoucherId == v.Id && u.Username == username);
            if (daDungCuaToi >= v.MoiNguoiToiDa)
                return Loi("Bạn đã dùng hết số lần cho phép của mã này.");

            return new VoucherResult
            {
                Ok = true,
                Message = "Áp dụng mã thành công.",
                Voucher = v,
                TienGiam = TinhGiam(v, tamTinh)
            };
        }

        private static VoucherResult Loi(string msg)
        {
            return new VoucherResult { Ok = false, Message = msg };
        }
    }
}