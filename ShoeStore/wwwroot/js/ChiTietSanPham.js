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

// 2. Chọn Size giày (Hỗ trợ gọi qua onclick="selectDetailSize(this)" và qua addEventListener)
function selectDetailSize(btn) {
    if (btn.classList.contains('disabled')) return;

    document.querySelectorAll('.size-options .btn-size').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');

    // Reset số lượng về 1 khi chọn size khác
    const inputQty = document.getElementById('inputQuantity');
    if (inputQty) {
        inputQty.value = 1;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const sizeBtns = document.querySelectorAll('.size-options .btn-size:not(.disabled)');
    sizeBtns.forEach(btn => {
        btn.addEventListener('click', () => selectDetailSize(btn));
    });
});

// 3. Tăng / Giảm số lượng mua theo tồn kho của Size đang chọn
function updateDetailQty(change) {
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

// Alias hỗ trợ hàm cũ nếu có gọi updateQty
function updateQty(change) {
    updateDetailQty(change);
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

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeSizeModal();
    }
});

// 5. Thêm sản phẩm vào giỏ hàng
function addCurrentDetailPageToCart() {
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

    const formData = new FormData();
    formData.append('maBienThe', maBienThe);
    formData.append('soLuong', quantity);

    fetch('/GioHang/ThemVaoGioHang', {
        method: 'POST',
        body: formData
    })
        .then(res => res.json())
        .then(res => {
            if (res.requireLogin) {
                alert(res.message);
                window.location.href = res.redirectUrl || '/TaiKhoan/DangNhap';
                return;
            }

            if (res.success) {
                const cartBadge = document.querySelector('.cart-badge');
                if (cartBadge && res.totalItems !== undefined) {
                    cartBadge.textContent = res.totalItems;
                }
                alert(res.message || "Đã thêm sản phẩm vào giỏ hàng!");
            } else {
                alert(res.message || "Không thể thêm vào giỏ hàng!");
            }
        })
        .catch(err => {
            console.error("Lỗi kết nối:", err);
            alert("Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng!");
        });
}

// Alias hỗ trợ nút bấm gọi addToCart
function addToCart(maGiay) {
    addCurrentDetailPageToCart();
}

// 6. Mua ngay (Chuyển hướng trực tiếp sang màn hình Thanh Toán)
function buyNowCurrentDetailPage() {
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

    window.location.href = `/DonHang/ThanhToan?maBienThe=${maBienThe}&soLuong=${quantity}`;
}

// Alias hỗ trợ hàm cũ buyNow
function buyNow(maGiay) {
    buyNowCurrentDetailPage();
}