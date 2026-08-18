/* ==========================================
   CHI TIẾT SẢN PHẨM JS (MINHEE SPORTS)
========================================== */

// 1. Đổi ảnh chính khi click vào ảnh Thumbnail nhỏ
function changeImage(element, src) {
    const mainImg = document.getElementById('mainImage');
    if (mainImg) {
        mainImg.src = src;
    }

    document.querySelectorAll('.thumb-item').forEach(el => el.classList.remove('active'));
    if (element) {
        element.classList.add('active');
    }
}

// 2. Chọn Size giày (Tự động kích hoạt cho các nút không bị Disabled)
document.addEventListener('DOMContentLoaded', () => {
    const sizeBtns = document.querySelectorAll('.size-options .btn-size:not(.disabled)');

    sizeBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            sizeBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            // Reset số lượng về 1 mỗi khi đổi size
            const inputQty = document.getElementById('inputQuantity');
            if (inputQty) {
                inputQty.value = 1;
            }
        });
    });
});

// 3. Tăng / Giảm số lượng mua (Kiểm tra theo tồn kho của Size đang chọn)
function updateQty(change) {
    const input = document.getElementById('inputQuantity');
    const activeSizeBtn = document.querySelector('.size-options .btn-size.active');

    if (!input) return;

    let currentVal = parseInt(input.value) || 1;
    let maxStock = activeSizeBtn ? (parseInt(activeSizeBtn.getAttribute('data-stock')) || 99) : 99;

    currentVal += change;

    if (currentVal < 1) {
        currentVal = 1;
    }

    if (currentVal > maxStock) {
        alert(`Size này hiện chỉ còn ${maxStock} sản phẩm trong kho!`);
        currentVal = maxStock;
    }

    input.value = currentVal;
}

// 4. Đóng / Mở Popup Modal Bảng Hướng Dẫn Chọn Size
function openSizeModal() {
    const modal = document.getElementById('sizeGuideModal');
    if (modal) {
        modal.style.setProperty('display', 'flex', 'important');
    } else {
        console.error("Lỗi: Không tìm thấy #sizeGuideModal trong HTML!");
    }
}

function closeSizeModal() {
    const modal = document.getElementById('sizeGuideModal');
    if (modal) {
        modal.style.setProperty('display', 'none', 'important');
    }
}

// Lắng nghe phím ESC để đóng Modal nhanh
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeSizeModal();
    }
});

// 5. Thêm sản phẩm vào giỏ hàng (Kết nối trực tiếp API Backend & SQL Server)
function addToCart(maGiay) {
    const activeSizeBtn = document.querySelector('.size-options .btn-size.active');
    const quantity = parseInt(document.getElementById('inputQuantity')?.value) || 1;

    if (!activeSizeBtn) {
        alert('Vui lòng chọn Size giày trước khi thêm vào giỏ hàng!');
        return;
    }

    const maBienThe = parseInt(activeSizeBtn.getAttribute('data-bienthe')) || 0;
    if (maBienThe <= 0) {
        alert('Size này hiện đang tạm hết hàng hoặc chưa có sẵn biến thể!');
        return;
    }

    fetch('/GioHang/ThemVaoGioHang', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            MaBienThe: maBienThe,
            SoLuong: quantity
        })
    })
        .then(res => res.json())
        .then(res => {
            if (res.requireLogin) {
                alert(res.message);
                window.location.href = res.redirectUrl || '/TaiKhoan/DangNhap';
                return;
            }

            if (res.success) {
                // Cập nhật số lượng trên icon giỏ hàng ở Header
                const cartBadge = document.querySelector('.cart-badge');
                if (cartBadge) {
                    cartBadge.textContent = res.totalItems;
                }
                alert(res.message);
            } else {
                alert(res.message);
            }
        })
        .catch(err => {
            console.error("Lỗi kết nối:", err);
            alert("Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng!");
        });
}

// 6. Mua ngay (Kiểm tra biến thể và chuyển hướng trực tiếp sang màn hình Thanh Toán)
function buyNow(maGiay) {
    const activeSizeBtn = document.querySelector('.size-options .btn-size.active');
    const quantity = parseInt(document.getElementById('inputQuantity')?.value) || 1;

    if (!activeSizeBtn) {
        alert('Vui lòng chọn Size giày trước khi mua ngay!');
        return;
    }

    const maBienThe = parseInt(activeSizeBtn.getAttribute('data-bienthe')) || 0;
    if (maBienThe <= 0) {
        alert('Size này hiện đang tạm hết hàng!');
        return;
    }

    // Chuyển hướng trực tiếp sang Action ThanhToan của DonHangController
    window.location.href = `/DonHang/ThanhToan?maBienThe=${maBienThe}&soLuong=${quantity}`;
}