using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wedsite_LTWW.Models
{
    public class CartItem
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}