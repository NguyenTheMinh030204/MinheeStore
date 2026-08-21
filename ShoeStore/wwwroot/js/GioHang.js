

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
                inputEl.value = newQty;

                const rowTotalEl = document.getElementById(`row-total-${maChiTiet}`);
                if (rowTotalEl) {
                    rowTotalEl.textContent = res.thanhTienMon;
                }

                tinhTongTienTamTinh();

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

function tienHanhThanhToan() {
    const checkedBoxes = document.querySelectorAll('.cart-card-item .cart-checkbox:checked');
    if (checkedBoxes.length === 0) {
        alert("Vui lòng chọn ít nhất một sản phẩm để thanh toán!");
        return;
    }

    const selectedIds = [];
    checkedBoxes.forEach(cb => {
        const row = cb.closest('.cart-card-item');
        if (row) {
            const id = row.getAttribute('data-id');
            if (id) selectedIds.push(id);
        }
    });

    if (selectedIds.length === 0) {
        alert("Không tìm thấy thông tin sản phẩm được chọn!");
        return;
    }

    window.location.href = `/DonHang/ThanhToan?ids=${selectedIds.join(',')}`;
}
