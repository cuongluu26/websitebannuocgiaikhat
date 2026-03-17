using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Wedsite_LTWW.Models;

namespace Wedsite_LTWW.Controllers
{
    public class HomeController : Controller
    {
        // Giả lập database
        List<SanPham> ds = new List<SanPham>()
{
    new SanPham{ Id=1, Ten="Nước Ép Dứa", Hinh="dua.jpg", Gia=15000 },
    new SanPham{ Id=2, Ten="Nước Ép Cam", Hinh="cam.jpg", Gia=18000 },
    new SanPham{ Id=3, Ten="Nước Ép Xoài", Hinh="xoai.jpg", Gia=20000 },
    new SanPham{ Id=4, Ten="Nước Ép Táo", Hinh="tao.jpg", Gia=17000 },
    new SanPham{ Id=5, Ten="Nước Ép Nho", Hinh="nho.jpg", Gia=22000 },
    new SanPham{ Id=6, Ten="Nước Ép Me", Hinh="me.jpg", Gia=16000 },
    new SanPham{ Id=7, Ten="Nước Ép Lê", Hinh="le.jpg", Gia=19000 },
    new SanPham{ Id=8, Ten="Nước Ép Dâu", Hinh="Dau.jpg", Gia=21000 },
    new SanPham{ Id=9, Ten="Nước Ép Mãng Cầu", Hinh="mangcau.jpg", Gia=23000 },
    new SanPham{ Id=10, Ten="Nước Ép Thanh Long", Hinh="thanhlong.jpg", Gia=25000 }
};

        // Trang chính (Load 4 sản phẩm đầu)
        public ActionResult Tranggiaodien(int page = 1)
        {
            int pageSize = 4;

            var data = ds
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(data);
        }

        // Action Load More (AJAX gọi tới đây)
        public ActionResult LoadMore(int page)
        {
            int pageSize = 4;

            var data = ds
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!data.Any())
            {
                return Content("");
            }

            return PartialView("_SanPhamPartial", data);
        }

        // Trang chi tiết sản phẩm
        public ActionResult SanPham(int id)
        {
            var sp = ds.FirstOrDefault(x => x.Id == id);
            return View(sp);
        }
    



    public ActionResult ChinhSachGiaoHang()
        {
            return View();
        }

        public ActionResult ChinhSachKiemHang()
        {
            return View();
        }
        public ActionResult ChinhSachĐoiTra()
        {
            return View();
        }
        public ActionResult ChinhSachThanhToan()
        {
            return View();
        }
        public ActionResult QuyMoVaThiTruong()
        {
            return View();
        }
    }
}


    
