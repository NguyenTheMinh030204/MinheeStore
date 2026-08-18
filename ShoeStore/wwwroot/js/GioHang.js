/* ====================================================
   MINHEE SPORTS - LOGIC GIỎ HÀNG CARD & CẬP NHẬT CSDL
==================================================== */

// 1. TĂNG / GIẢM SỐ LƯỢNG SẢN PHẨM TRONG CSDL
function thayDoiSoLuong(maChiTiet, delta, maxStock) {
    const inputEl = document.getElementById(`qty-input-${maChiTiet}`);
    if (!inputEl) return;

    let currentQty = parseInt(inputEl.value) || 1;
    let newQty = currentQty + delta;

    if (newQty < 1) {
        if (confirm("Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?")) {
            xoaSanPham(maChiTiet);
        }
        return;
    }

    if (maxStock > 0 && newQty > maxStock) {
        alert(`Số lượng vượt quá tồn kho! Size này hiện chỉ còn ${maxStock} đôi.`);
        return;
    }

    // Gửi Form dữ liệu lên CSDL qua Action CapNhatSoLuong
    const formData = new FormData();
    formData.append('maChiTiet', maChiTiet);
    formData.append('soLuong', newQty);

    fetch('/GioHang/CapNhatSoLuong', {
        method: 'POST',
        body: formData
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
                // Cập nhật số lượng trên ô input
                inputEl.value = newQty;

                // Cập nhật thành tiền riêng của dòng đó
                const rowTotalEl = document.getElementById(`row-total-${maChiTiet}`);
                if (rowTotalEl) {
                    rowTotalEl.textContent = res.thanhTienMon;
                }

                // Tính toán lại tổng tiền các món được tích chọn
                tinhTongTienTamTinh();

                // Cập nhật badge số lượng ở header
                const cartBadge = document.querySelector('.cart-badge');
                if (cartBadge) {
                    cartBadge.textContent = res.totalItems;
                }
            } else {
                alert(res.message || "Không thể cập nhật số lượng!");
            }
        })
        .catch(err => {
            console.error("Lỗi:", err);
            alert("Đã xảy ra lỗi khi cập nhật số lượng vào CSDL!");
        });
}

// 2. XÓA SẢN PHẨM KHỎI CSDL
function xoaSanPham(maChiTiet) {
    if (!confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?")) {
        return;
    }

    const formData = new FormData();
    formData.append('maChiTiet', maChiTiet);

    fetch('/GioHang/XoaKhoiGioHang', {
        method: 'POST',
        body: formData
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
                const rowEl = document.getElementById(`cart-item-${maChiTiet}`);
                if (rowEl) {
                    rowEl.style.opacity = '0';
                    rowEl.style.transform = 'scale(0.95)';
                    setTimeout(() => {
                        rowEl.remove();

                        // Nếu đã xóa hết sản phẩm
                        if (res.isEmpty) {
                            const mainContent = document.getElementById('cartMainContent');
                            const emptyState = document.getElementById('cartEmptyState');
                            if (mainContent) mainContent.remove();
                            if (emptyState) emptyState.style.display = 'flex';
                        } else {
                            tinhTongTienTamTinh();
                        }
                    }, 200);
                }

                // Cập nhật badge header
                const cartBadge = document.querySelector('.cart-badge');
                if (cartBadge) {
                    cartBadge.textContent = res.totalItems;
                }
            } else {
                alert(res.message || "Không thể xóa sản phẩm.");
            }
        })
        .catch(err => {
            console.error("Lỗi:", err);
            alert("Đã xảy ra lỗi khi xóa sản phẩm!");
        });
}

// 3. TÍNH LẠI TỔNG TIỀN THEO CHECKBOX ĐƯỢC CHỌN
function tinhTongTienTamTinh() {
    let tongTien = 0;
    let tongSoLuong = 0;

    const allCards = document.querySelectorAll('.cart-card-item');
    allCards.forEach(card => {
        const checkbox = card.querySelector('.cart-checkbox');
        if (checkbox && checkbox.checked) {
            const qtyInput = card.querySelector('.input-qty-value');
            const rowTotal = card.querySelector('.card-item-total-price');
            const unitPrice = parseFloat(rowTotal.getAttribute('data-price')) || 0;
            const qty = parseInt(qtyInput.value) || 0;

            tongSoLuong += qty;
            tongTien += (unitPrice * qty);
        }
    });

    const countEl = document.getElementById('summaryTotalCount');
    const grandTotalEl = document.getElementById('summaryGrandTotal');

    if (countEl) countEl.textContent = tongSoLuong;
    if (grandTotalEl) grandTotalEl.textContent = tongTien.toLocaleString('vi-VN') + ' VNĐ';
}

// 4. TIẾN HÀNH THANH TOÁN
function tienHanhThanhToan() {
    window.location.href = '/DonHang/ThanhToan';
}