document.addEventListener('DOMContentLoaded', () => {

    const bannerSection = document.querySelector('.hero-banner');
    const sliderContainer = document.querySelector('.hero-banner .banner-slider');
    const slides = document.querySelectorAll('.hero-banner .slide');
    const dots = document.querySelectorAll('.banner-dots .dot');

    if (!bannerSection || slides.length === 0) return;

    let currentIndex = 0;
    let slideInterval = null;
    const autoPlayTime = 4000; 

    function showSlide(index) {
        if (index >= slides.length) {
            currentIndex = 0;
        } else if (index < 0) {
            currentIndex = slides.length - 1;
        } else {
            currentIndex = index;
        }

        slides.forEach((slide, idx) => {
            slide.classList.toggle('active', idx === currentIndex);
        });

        dots.forEach((dot, idx) => {
            dot.classList.toggle('active', idx === currentIndex);
        });
    }

    function nextSlide() {
        showSlide(currentIndex + 1);
    }

    function startAutoSlide() {
        if (slides.length > 1 && !slideInterval) {
            slideInterval = setInterval(nextSlide, autoPlayTime);
        }
    }

    function stopAutoSlide() {
        if (slideInterval) {
            clearInterval(slideInterval);
            slideInterval = null;
        }
    }

    function resetAutoSlide() {
        stopAutoSlide();
        startAutoSlide();
    }

    dots.forEach(dot => {
        dot.addEventListener('click', (e) => {
            const targetIndex = parseInt(e.target.getAttribute('data-index')) || 0;
            if (targetIndex !== currentIndex) {
                showSlide(targetIndex);
                resetAutoSlide();
            }
        });
    });

    bannerSection.addEventListener('mouseenter', stopAutoSlide);
    bannerSection.addEventListener('mouseleave', startAutoSlide);

    showSlide(0);
    startAutoSlide();
});
