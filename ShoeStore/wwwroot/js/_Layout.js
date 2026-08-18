/* ====================================================
   MINHEE SPORTS - LAYOUT JS (GLOBAL)
==================================================== */

document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. TỰ ĐỘNG ACTIVE MENU NAV (CHUẨN 100%)
    // ==========================================
    const currentPath = window.location.pathname.toLowerCase();
    const currentParams = new URLSearchParams(window.location.search);
    const currentId = (currentParams.get('id') || currentParams.get('maloai') || '').toLowerCase();

    const navItems = document.querySelectorAll('.main-nav .nav-item');
    navItems.forEach(item => item.classList.remove('active'));

    let isMatched = false;

    // 1. Khớp trang Danh mục có Query String (?id=BeTrai, ?id=HotSale,...)
    if (currentId) {
        navItems.forEach(item => {
            const href = (item.getAttribute('href') || '').toLowerCase();
            if (href.includes('id=' + currentId) || href.includes('maloai=' + currentId)) {
                item.classList.add('active');
                isMatched = true;
            }
        });
    }

    // 2. Khớp Trang Chủ (URL là '/', '/trangchu', '/trangchu/trangchu')
    if (!isMatched && (currentPath === '/' || currentPath === '' || currentPath.includes('/trangchu'))) {
        navItems.forEach(item => {
            const href = (item.getAttribute('href') || '').toLowerCase();
            if (href === '/' || href.includes('trangchu')) {
                item.classList.add('active');
                isMatched = true;
            }
        });
    }

    // ==========================================
    // 2. TÍNH NĂNG TÌM KIẾM DÙNG CHUNG (LIVE SEARCH)
    // ==========================================
    const searchInput = document.getElementById('searchInput');
    const searchResultsPopup = document.getElementById('searchResultsPopup');
    const searchItemsList = document.getElementById('searchItemsList');
    const totalSearchCount = document.getElementById('totalSearchCount');
    const viewAllSearchLink = document.getElementById('viewAllSearchLink');
    const searchBtn = document.getElementById('searchBtn');

    let searchTimer = null;

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const keyword = e.target.value.trim();
            clearTimeout(searchTimer);

            if (keyword.length < 2) {
                if (searchResultsPopup) searchResultsPopup.style.display = 'none';
                return;
            }

            searchTimer = setTimeout(() => {
                fetch(`/TimKiem/TimKiemNhanh?keyword=${encodeURIComponent(keyword)}`)
                    .then(res => res.json())
                    .then(resData => {
                        if (resData.success && resData.data && resData.data.length > 0) {
                            renderSearchPopup(resData.data, resData.total, keyword);
                        } else {
                            renderSearchEmpty();
                        }
                    })
                    .catch(err => console.error("Lỗi Live Search:", err));
            }, 300);
        });

        function executeSearch() {
            const kw = searchInput.value.trim();
            if (kw) {
                window.location.href = `/TimKiem/KetQua?search=${encodeURIComponent(kw)}`;
            }
        }

        if (searchBtn) {
            searchBtn.addEventListener('click', executeSearch);
        }

        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                executeSearch();
            }
        });

        document.addEventListener('click', (e) => {
            if (!e.target.closest('.search-box-wrapper')) {
                if (searchResultsPopup) searchResultsPopup.style.display = 'none';
            }
        });
    }

    function renderSearchPopup(items, total, keyword) {
        if (totalSearchCount) totalSearchCount.textContent = total;
        if (viewAllSearchLink) viewAllSearchLink.href = `/TimKiem/KetQua?search=${encodeURIComponent(keyword)}`;

        let html = '';
        items.forEach(item => {
            const id = item.id || item.maGiay || item.MaGiay;
            const ten = item.ten || item.tenGiay || item.TenGiay;
            const anh = item.anh || item.anhChinh || item.AnhChinh;
            const giaB = item.giaBan || item.GiaBan || 0;
            const giaC = item.giaCu || item.GiaCu;

            const giaBan = new Intl.NumberFormat('vi-VN').format(giaB) + ' VNĐ';
            const giaCu = giaC ? new Intl.NumberFormat('vi-VN').format(giaC) + ' VNĐ' : '';

            html += `
                <a href="/Giay/ChiTiet?id=${id}" class="search-item">
                    <img src="${anh}" alt="${ten}" class="search-item-img">
                    <div class="search-item-info">
                        <div class="search-item-name">${ten}</div>
                        <div class="search-item-prices">
                            <span class="search-item-price-current">${giaBan}</span>
                            ${giaC ? `<span class="search-item-price-old">${giaCu}</span>` : ''}
                        </div>
                    </div>
                </a>
            `;
        });

        if (searchItemsList) searchItemsList.innerHTML = html;
        if (searchResultsPopup) searchResultsPopup.style.display = 'block';
    }

    function renderSearchEmpty() {
        if (totalSearchCount) totalSearchCount.textContent = '0';
        if (searchItemsList) {
            searchItemsList.innerHTML = `<div class="search-no-result">Không tìm thấy sản phẩm phù hợp</div>`;
        }
        if (searchResultsPopup) searchResultsPopup.style.display = 'block';
    }

    // ==========================================
    // 3. TẢI TỔNG SỐ LƯỢNG GIỎ HÀNG (BADGE)
    // ==========================================
    window.loadCartBadge();
});

// ====================================================
// 4. LOGIC POPUP CHỌN SIZE & THÊM GIỎ / MUA NGAY (GLOBAL)
// ====================================================
let currentSelectedBienThe = 0;
let currentMaxStock = 0;

window.loadCartBadge = function () {
    fetch('/GioHang/LayTongSoLuongGioHang')
        .then(res => res.json())
        .then(data => {
            const cartBadge = document.querySelector('.cart-badge');
            if (cartBadge) {
                cartBadge.textContent = data.totalItems || 0;
            }
        })
        .catch(err => console.error("Lỗi lấy giỏ hàng:", err));
};

window.addToCart = function (maGiay) {
    if (!maGiay) {
        console.error("Mã giày trống!");
        return;
    }

    fetch(`/GioHang/LayThongTinNhanh?maGiay=${encodeURIComponent(maGiay)}`)
        .then(async res => {
            if (!res.ok) {
                const txt = await res.text();
                throw new Error(`HTTP ${res.status}: ${txt}`);
            }
            return res.json();
        })
        .then(res => {
            if (!res.success) {
                alert(res.message);
                return;
            }

            const data = res.data;
            const imgEl = document.getElementById('quickCartImg');
            const titleEl = document.getElementById('quickCartTitle');
            const priceEl = document.getElementById('quickCartPrice');
            const qtyInput = document.getElementById('quickCartQuantity');
            const sizeContainer = document.getElementById('quickCartSizes');
            const modalEl = document.getElementById('quickAddToCartModal');

            if (imgEl) imgEl.src = data.anhChinh;
            if (titleEl) titleEl.textContent = data.tenGiay;
            if (priceEl) priceEl.textContent = Number(data.giaBan).toLocaleString('vi-VN') + ' VNĐ';
            if (qtyInput) qtyInput.value = 1;

            let sizeHtml = '';
            let hasFirstActive = false;
            currentSelectedBienThe = 0;
            currentMaxStock = 0;

            if (data.sizes && data.sizes.length > 0) {
                data.sizes.forEach(s => {
                    let activeClass = '';
                    let disabledAttr = '';

                    if (s.conHang) {
                        if (!hasFirstActive) {
                            activeClass = 'active';
                            hasFirstActive = true;
                            currentSelectedBienThe = s.maBienThe;
                            currentMaxStock = s.tonKho;
                        }
                    } else {
                        disabledAttr = 'disabled';
                    }

                    sizeHtml += `
                        <button type="button" 
                                class="btn-quick-size ${activeClass} ${!s.conHang ? 'disabled' : ''}" 
                                data-bienthe="${s.maBienThe}" 
                                data-stock="${s.tonKho}"
                                onclick="selectQuickSize(this)"
                                ${disabledAttr}>
                            ${s.size}
                        </button>
                    `;
                });
            }

            if (sizeContainer) sizeContainer.innerHTML = sizeHtml;
            if (modalEl) modalEl.style.setProperty('display', 'flex', 'important');
        })
        .catch(err => {
            console.error("Lỗi LayThongTinNhanh:", err);
            alert("Lỗi tải thông tin sản phẩm: " + err.message);
        });
};

window.selectQuickSize = function (element) {
    document.querySelectorAll('.btn-quick-size').forEach(b => b.classList.remove('active'));
    element.classList.add('active');

    currentSelectedBienThe = parseInt(element.getAttribute('data-bienthe')) || 0;
    currentMaxStock = parseInt(element.getAttribute('data-stock')) || 0;
    const qtyInput = document.getElementById('quickCartQuantity');
    if (qtyInput) qtyInput.value = 1;
};

window.updateQuickQty = function (delta) {
    const input = document.getElementById('quickCartQuantity');
    if (!input) return;
    let qty = parseInt(input.value) || 1;
    qty += delta;

    if (qty < 1) qty = 1;
    if (currentMaxStock > 0 && qty > currentMaxStock) {
        alert(`Size này trong kho chỉ còn tối đa ${currentMaxStock} đôi!`);
        qty = currentMaxStock;
    }
    input.value = qty;
};

// ====================================================
// ĐOẠN ĐÃ ĐƯỢC BỔ SUNG TRUY VẾT LỖI CHÍNH XÁC
// ====================================================
window.confirmAddToCart = function () {
    if (currentSelectedBienThe <= 0) {
        alert('Vui lòng chọn Size giày còn hàng!');
        return;
    }

    const qty = parseInt(document.getElementById('quickCartQuantity')?.value) || 1;

    // Gửi dữ liệu dạng FormData (khớp chuẩn với action ThemVaoGioHang(int maBienThe, int soLuong))
    const formData = new FormData();
    formData.append('maBienThe', currentSelectedBienThe);
    formData.append('soLuong', qty);

    fetch('/GioHang/ThemVaoGioHang', {
        method: 'POST',
        body: formData
    })
        .then(async res => {
            // Kiểm tra xem phản hồi có thành công (Status 200-299) không
            if (!res.ok) {
                const rawErrorText = await res.text();
                throw new Error(`[Mã phản hồi HTTP ${res.status}] ${rawErrorText}`);
            }

            // Kiểm tra định dạng có phải JSON hay bị trả về trang HTML Đăng Nhập
            const contentType = res.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                const htmlText = await res.text();
                throw new Error(`Server không trả về JSON mà trả về trang HTML (khả năng bị Redirect sang trang Đăng Nhập). Nội dung: ${htmlText.substring(0, 150)}...`);
            }

            return res.json();
        })
        .then(res => {
            if (res.requireLogin) {
                alert(res.message || "Vui lòng đăng nhập để tiếp tục!");
                window.location.href = res.redirectUrl || '/TaiKhoan/DangNhap';
                return;
            }

            if (res.success) {
                alert(res.message || "Thêm vào giỏ hàng thành công!");
                window.closeQuickCartModal();

                const cartBadge = document.querySelector('.cart-badge');
                if (cartBadge) {
                    cartBadge.textContent = res.totalItems;
                }
            } else {
                // In thông báo lỗi logic từ Controller gửi về (Ví dụ: hết hàng, dữ liệu không hợp lệ...)
                alert("Thông báo từ hệ thống: " + (res.message || "Không thể thêm vào giỏ."));
            }
        })
        .catch(err => {
            // In đầy đủ chi tiết lỗi kỹ thuật
            console.error("Chi tiết lỗi thêm giỏ hàng:", err);
            alert("CHI TIẾT LỖI GẶP PHẢI:\n" + err.message);
        });
};

window.confirmBuyNow = function () {
    if (currentSelectedBienThe <= 0) {
        alert('Vui lòng chọn Size giày còn hàng!');
        return;
    }

    const qty = parseInt(document.getElementById('quickCartQuantity')?.value) || 1;
    window.location.href = `/DonHang/ThanhToan?maBienThe=${currentSelectedBienThe}&soLuong=${qty}`;
};

window.closeQuickCartModal = function () {
    const modal = document.getElementById('quickAddToCartModal');
    if (modal) {
        modal.style.setProperty('display', 'none', 'important');
    }
};

// ====================================================
// 5. LOGIC POPUP HƯỚNG DẪN CHỌN SIZE GIÀY (GLOBAL)
// ====================================================
window.openSizeModal = function () {
    const modal = document.getElementById('sizeGuideModal');
    if (modal) {
        modal.style.setProperty('display', 'flex', 'important');
    }
};

window.closeSizeModal = function () {
    const modal = document.getElementById('sizeGuideModal');
    if (modal) {
        modal.style.setProperty('display', 'none', 'important');
    }
};