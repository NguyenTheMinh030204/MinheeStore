document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // LOGIC BANNER SLIDE TỰ ĐỘNG (CHỈ TRANG CHỦ)
    // ==========================================
    const slides = document.querySelectorAll('.hero-banner .slide');
    const dots = document.querySelectorAll('.banner-dots .dot');
    const bannerSection = document.querySelector('.hero-banner');

    // Nếu trang hiện tại không có Banner Slider thì không chạy code dưới
    if (!bannerSection || slides.length === 0) return;

    let currentIndex = 0;
    let slideInterval;
    const autoPlayTime = 5000;

    function showSlide(index) {
        if (index >= slides.length) {
            currentIndex = 0;
        } else if (index < 0) {
            currentIndex = slides.length - 1;
        } else {
            currentIndex = index;
        }

        slides.forEach(slide => slide.classList.remove('active'));
        dots.forEach(dot => dot.classList.remove('active'));

        slides[currentIndex].classList.add('active');
        if (dots[currentIndex]) {
            dots[currentIndex].classList.add('active');
        }
    }

    function nextSlide() {
        showSlide(currentIndex + 1);
    }

    function startAutoSlide() {
        if (slides.length > 1) {
            slideInterval = setInterval(nextSlide, autoPlayTime);
        }
    }

    function stopAutoSlide() {
        clearInterval(slideInterval);
    }

    function resetAutoSlide() {
        stopAutoSlide();
        startAutoSlide();
    }

    dots.forEach(dot => {
        dot.addEventListener('click', (e) => {
            const index = parseInt(e.target.getAttribute('data-index'));
            showSlide(index);
            resetAutoSlide();
        });
    });

    bannerSection.addEventListener('mouseenter', stopAutoSlide);
    bannerSection.addEventListener('mouseleave', startAutoSlide);

    // Kích hoạt chạy slider
    startAutoSlide();
});