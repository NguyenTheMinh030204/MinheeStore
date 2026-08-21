using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class ThongTinController : Controller
    {
        
        [HttpGet]
        [Route("gioi-thieu")]
        [Route("ThongTin/GioiThieu")]
        public IActionResult GioiThieu()
        {
            return View("~/Views/ThongTin/GioiThieu.cshtml");
        }

        [HttpGet]
        [Route("huong-dan-chon-size")]
        [Route("ThongTin/HuongDanChonSize")]
        public IActionResult HuongDanChonSize()
        {
            return View("~/Views/ThongTin/HuongDanChonSize.cshtml");
        }

        [HttpGet]
        [Route("chinh-sach-doi-tra")]
        [Route("chinh-sach-doi-tra-bao-hanh")]
        [Route("ThongTin/ChinhSachDoiTra")]
        public IActionResult ChinhSachDoiTra()
        {
            return View("~/Views/ThongTin/ChinhSachDoiTra.cshtml");
        }

        [HttpGet]
        [Route("huong-dan-thanh-toan")]
        [Route("ThongTin/HuongDanThanhToan")]
        public IActionResult HuongDanThanhToan()
        {
            return View("~/Views/ThongTin/HuongDanThanhToan.cshtml");
        }

        [HttpGet]
        [Route("chinh-sach-bao-mat")]
        [Route("ThongTin/ChinhSachBaoMat")]
        public IActionResult ChinhSachBaoMat()
        {
            return View("~/Views/ThongTin/ChinhSachBaoMat.cshtml");
        }

        [HttpGet]
        [Route("chinh-sach-van-chuyen")]
        [Route("ThongTin/ChinhSachVanChuyen")]
        public IActionResult ChinhSachVanChuyen()
        {
            return View("~/Views/ThongTin/ChinhSachVanChuyen.cshtml");
        }

        [HttpGet]
        [Route("chinh-sach-kiem-hang")]
        [Route("ThongTin/ChinhSachKiemHang")]
        public IActionResult ChinhSachKiemHang()
        {
            return View("~/Views/ThongTin/ChinhSachKiemHang.cshtml");
        }

        [HttpGet]
        [Route("chinh-sach-dich-vu")]
        [Route("ThongTin/ChinhSachDichVu")]
        public IActionResult ChinhSachDichVu()
        {
            return View("~/Views/ThongTin/ChinhSachDichVu.cshtml");
        }
    }
}
