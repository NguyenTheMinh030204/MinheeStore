function chonPhuongThucThanhToan(radioEl) {
    document.querySelectorAll('.payment-option').forEach(el => el.classList.remove('active'));
    radioEl.closest('.payment-option').classList.add('active');
}

function thucHienDatHang() {
    const hoTen = document.getElementById('txtHoTen').value.trim();
    const sdt = document.getElementById('txtSDT').value.trim();
    const email = document.getElementById('txtEmail').value.trim();
    const diaChi = document.getElementById('txtDiaChi').value.trim();
    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked')?.value || "COD";

    if (!hoTen) {
        alert("Vui lòng nhập họ và tên người nhận!");
        document.getElementById('txtHoTen').focus();
        return;
    }
    if (!sdt) {
        alert("Vui lòng nhập số điện thoại nhận hàng!");
        document.getElementById('txtSDT').focus();
        return;
    }
    if (!diaChi) {
        alert("Vui lòng nhập địa chỉ giao hàng chi tiết!");
        document.getElementById('txtDiaChi').focus();
        return;
    }

    const urlParams = new URLSearchParams(window.location.search);
    const maBienThe = urlParams.get('maBienThe');
    const soLuong = urlParams.get('soLuong');
    const ids = urlParams.get('ids');

    const payload = {
        hoTenNguoiNhan: hoTen,
        emailNguoiNhan: email,
        soDienThoaiNhan: sdt,
        diaChiGiaoHang: diaChi,
        phuongThucThanhToan: paymentMethod,
        phiVanChuyen: 30000
    };

    if (maBienThe && soLuong) {
        payload.maBienTheMuaNgay = parseInt(maBienThe);
        payload.soLuongMuaNgay = parseInt(soLuong);
    } else if (ids) {
        payload.danhSachMaChiTietChon = ids.split(',').map(x => parseInt(x));
    }

    const btn = document.querySelector('.btn-confirm-order');
    btn.disabled = true;
    btn.innerHTML = `<span class="material-icons-outlined spin">autorenew</span> Đang xử lý...`;

    fetch('/DonHang/XacNhanDatHang', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(async res => {
            if (!res.ok) {
                const err = await res.text();
                throw new Error(`HTTP ${res.status}: ${err}`);
            }
            return res.json();
        })
        .then(res => {
            if (res.success) {
                alert(res.message);
                window.location.href = res.redirectUrl;
            } else {
                alert(res.message || "Đặt hàng thất bại!");
                btn.disabled = false;
                btn.innerHTML = `<span class="material-icons-outlined">lock</span> XÁC NHẬN ĐẶT HÀNG`;
            }
        })
        .catch(err => {
            console.error("Lỗi đặt hàng:", err);
            alert("Đã xảy ra lỗi khi tạo đơn hàng. Vui lòng thử lại!");
            btn.disabled = false;
            btn.innerHTML = `<span class="material-icons-outlined">lock</span> XÁC NHẬN ĐẶT HÀNG`;
        });
}
