document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. TỰ ĐỘNG ACTIVE MENU NAV THEO URL CURRENT
    // ==========================================
    const currentPath = window.location.pathname.toLowerCase();
    const currentParams = new URLSearchParams(window.location.search);

    // Lấy giá trị tham số danh mục trên URL (hỗ trợ cả 'id' lẫn 'maloai')
    const currentId = (currentParams.get('id') || currentParams.get('maloai') || '').toLowerCase();

    const navItems = document.querySelectorAll('.main-nav .nav-item');

    // Xóa toàn bộ class active cũ
    navItems.forEach(item => item.classList.remove('active'));

    let isMatched = false;

    navItems.forEach(item => {
        const href = item.getAttribute('href');
        if (!href) return;

        // Tách đường dẫn thô và tham số query từ href của thẻ <a>
        const hrefParts = href.toLowerCase().split('?');
        const itemPath = hrefParts[0] || '';
        const itemParams = new URLSearchParams(hrefParts[1] || '');
        const itemId = (itemParams.get('id') || itemParams.get('maloai') || '').toLowerCase();

        // TRƯỜNG HỢP 1: Trang Danh mục / HotSale có tham số ID (?id=BeTrai hoặc ?maloai=BeTrai)
        if (currentId && itemId) {
            if (currentId === itemId) {
                item.classList.add('active');
                isMatched = true;
            }
        }
        // TRƯỜNG HỢP 2: Các trang tĩnh không tham số query (vd: /TrangChu/TrangChu)
        else if (!currentId && !itemId) {
            if (itemPath === currentPath && itemPath !== '/' && !itemPath.includes('/trangchu')) {
                item.classList.add('active');
                isMatched = true;
            }
        }
    });

    // Mặc định Active "Trang chủ" nếu đang ở trang gốc "/" hoặc "/TrangChu/TrangChu" mà chưa khớp mục nào
    if (!isMatched && (currentPath === '/' || currentPath.includes('/trangchu'))) {
        const homeNav = document.querySelector('.main-nav a[href*="TrangChu"]') ||
            document.querySelector('.main-nav a[href="/"]');
        if (homeNav) homeNav.classList.add('active');
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
        // Gõ phím kích hoạt tìm kiếm gợi ý nhanh
        searchInput.addEventListener('input', (e) => {
            const keyword = e.target.value.trim();
            clearTimeout(searchTimer);

            if (keyword.length < 2) {
                if (searchResultsPopup) searchResultsPopup.style.display = 'none';
                return;
            }

            // Chờ 300ms dừng gõ mới bắn API tránh giật lag Server
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

        // Chuyển hướng sang màn hình Kết quả tìm kiếm đầy đủ
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

        // Click ra ngoài vùng search box tự động ẩn Popup
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.search-box-wrapper')) {
                if (searchResultsPopup) searchResultsPopup.style.display = 'none';
            }
        });
    }

    // Render HTML Popup gợi ý (ĐÃ SỬA CÂU LINK CHUẨN)
    function renderSearchPopup(items, total, keyword) {
        if (totalSearchCount) totalSearchCount.textContent = total;
        if (viewAllSearchLink) viewAllSearchLink.href = `/TimKiem/KetQua?search=${encodeURIComponent(keyword)}`;

        let html = '';
        items.forEach(item => {
            // Lấy ID/Mã giày linh hoạt
            const id = item.id || item.maGiay || item.MaGiay;
            const ten = item.ten || item.tenGiay || item.TenGiay;
            const anh = item.anh || item.anhChinh || item.AnhChinh;
            const giaB = item.giaBan || item.GiaBan || 0;
            const giaC = item.giaCu || item.GiaCu;

            const giaBan = new Intl.NumberFormat('vi-VN').format(giaB) + ' VNĐ';
            const giaCu = giaC ? new Intl.NumberFormat('vi-VN').format(giaC) + ' VNĐ' : '';

            // Link đã được sửa thành /Giay/ChiTiet?id=...
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

    // Render trạng thái không có sản phẩm
    function renderSearchEmpty() {
        if (totalSearchCount) totalSearchCount.textContent = '0';
        if (searchItemsList) {
            searchItemsList.innerHTML = `<div class="search-no-result" style="padding: 15px; text-align: center; color: #64748b; font-size: 13px;">Không tìm thấy sản phẩm phù hợp</div>`;
        }
        if (searchResultsPopup) searchResultsPopup.style.display = 'block';
    }

    // ==========================================
    // 3. HIỆU ỨNG THÊM VÀO GIỎ HÀNG (DÙNG CHUNG)
    // ==========================================
    let cartCount = 0;
    const cartBadge = document.querySelector('.cart-badge');

    // Dùng Event Delegation để bắt sự kiện click cho cả các nút được render động
    document.addEventListener('click', (e) => {
        const button = e.target.closest('.btn-add-cart');
        if (!button) return;

        cartCount++;
        if (cartBadge) cartBadge.textContent = cartCount;

        button.style.backgroundColor = 'var(--semantic-green, #10B981)';
        const originalText = button.innerHTML;
        button.innerHTML = `<span class="material-icons-outlined">check</span> Đã thêm`;

        setTimeout(() => {
            button.style.backgroundColor = '';
            button.innerHTML = originalText;
        }, 1200);
    });
});