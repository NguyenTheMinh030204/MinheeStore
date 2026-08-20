using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class ThongTinController : Controller
    {
        // 1. Giới thiệu thương hiệu
        [HttpGet]
        [Route("gioi-thieu")]
        [Route("ThongTin/GioiThieu")]
        public IActionResult GioiThieu()
        {
            return View("~/Views/ThongTin/GioiThieu.cshtml");
        }

        // 2. Hướng dẫn chọn size giày
        [HttpGet]
        [Route("huong-dan-chon-size")]
        [Route("ThongTin/HuongDanChonSize")]
        public IActionResult HuongDanChonSize()
        {
            return View("~/Views/ThongTin/HuongDanChonSize.cshtml");
        }

        // 3. Chính sách đổi trả hàng & bảo hành
        [HttpGet]
        [Route("chinh-sach-doi-tra")]
        [Route("chinh-sach-doi-tra-bao-hanh")]
        [Route("ThongTin/ChinhSachDoiTra")]
        public IActionResult ChinhSachDoiTra()
        {
            return View("~/Views/ThongTin/ChinhSachDoiTra.cshtml");
        }

        // 4. Hướng dẫn thanh toán
        [HttpGet]
        [Route("huong-dan-thanh-toan")]
        [Route("ThongTin/HuongDanThanhToan")]
        public IActionResult HuongDanThanhToan()
        {
            return View("~/Views/ThongTin/HuongDanThanhToan.cshtml");
        }

        // 5. Chính sách bảo mật thông tin
        [HttpGet]
        [Route("chinh-sach-bao-mat")]
        [Route("ThongTin/ChinhSachBaoMat")]
        public IActionResult ChinhSachBaoMat()
        {
            return View("~/Views/ThongTin/ChinhSachBaoMat.cshtml");
        }

        // 6. Chính sách giao hàng & vận chuyển
        [HttpGet]
        [Route("chinh-sach-van-chuyen")]
        [Route("ThongTin/ChinhSachVanChuyen")]
        public IActionResult ChinhSachVanChuyen()
        {
            return View("~/Views/ThongTin/ChinhSachVanChuyen.cshtml");
        }

        // 7. Chính sách đồng kiểm hàng hóa
        [HttpGet]
        [Route("chinh-sach-kiem-hang")]
        [Route("ThongTin/ChinhSachKiemHang")]
        public IActionResult ChinhSachKiemHang()
        {
            return View("~/Views/ThongTin/ChinhSachKiemHang.cshtml");
        }

        // 8. Chính sách & tiêu chuẩn dịch vụ
        [HttpGet]
        [Route("chinh-sach-dich-vu")]
        [Route("ThongTin/ChinhSachDichVu")]
        public IActionResult ChinhSachDichVu()
        {
            return View("~/Views/ThongTin/ChinhSachDichVu.cshtml");
        }
    }
}