document.addEventListener('DOMContentLoaded', () => {

    // ==========================================
    // 1. TÍNH NĂNG ẨN / HIỆN MẬT KHẨU
    // ==========================================
    const toggleButtons = document.querySelectorAll('.toggle-password');

    toggleButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.getAttribute('data-target');
            const targetInput = document.getElementById(targetId);

            if (targetInput) {
                if (targetInput.type === 'password') {
                    targetInput.type = 'text';
                    btn.textContent = 'visibility_off';
                } else {
                    targetInput.type = 'password';
                    btn.textContent = 'visibility';
                }
            }
        });
    });

    // ==========================================
    // 2. TỰ ĐỘNG NHẢY 6 Ô NHẬP MÃ OTP
    // ==========================================
    const otpInputs = document.querySelectorAll('.otp-input');
    const fullOtpInput = document.getElementById('fullOtpCode');

    if (otpInputs.length > 0) {
        otpInputs.forEach((input, index) => {

            input.addEventListener('input', (e) => {
                const val = e.target.value;

                // Chỉ cho nhập số
                if (!/^\d*$/.test(val)) {
                    e.target.value = '';
                    return;
                }

                if (val.length === 1 && index < otpInputs.length - 1) {
                    otpInputs[index + 1].focus();
                }

                updateFullOtp();
            });

            input.addEventListener('keydown', (e) => {
                if (e.key === 'Backspace' && !input.value && index > 0) {
                    otpInputs[index - 1].focus();
                }
            });
        });

        function updateFullOtp() {
            let otpStr = '';
            otpInputs.forEach(inp => otpStr += inp.value);
            if (fullOtpInput) fullOtpInput.value = otpStr;
        }
    }

    // ==========================================
    // 3. XỬ LÝ NÚT LẤY OTP VÀ ĐẾM NGƯỢC (GỬI EMAIL THẬT)
    // ==========================================
    const btnSendOtp = document.getElementById('btnSendOtp');
    const regEmail = document.getElementById('regEmail');

    if (btnSendOtp) {
        btnSendOtp.addEventListener('click', () => {
            const email = regEmail ? regEmail.value.trim() : '';
            if (!email) {
                alert('Vui lòng nhập địa chỉ Email trước khi lấy mã OTP!');
                if (regEmail) regEmail.focus();
                return;
            }

            // Vô hiệu hóa nút trong lúc gửi Mail
            btnSendOtp.disabled = true;
            btnSendOtp.style.opacity = '0.6';
            btnSendOtp.textContent = 'Đang gửi...';

            // Gọi API /TaiKhoan/GuiOTP trong TaiKhoanController
            fetch(`/TaiKhoan/GuiOTP?email=${encodeURIComponent(email)}`, { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message || 'Mã OTP đã được gửi về Email của bạn!');

                        // Đếm ngược 60s
                        let countdown = 60;
                        const timer = setInterval(() => {
                            btnSendOtp.textContent = `Thử lại (${countdown}s)`;
                            countdown--;

                            if (countdown < 0) {
                                clearInterval(timer);
                                btnSendOtp.disabled = false;
                                btnSendOtp.style.opacity = '1';
                                btnSendOtp.textContent = 'Lấy OTP';
                            }
                        }, 1000);
                    } else {
                        // Trường hợp Email đã đăng ký hoặc không hợp lệ
                        alert(data.message || 'Lỗi gửi mã OTP!');
                        btnSendOtp.disabled = false;
                        btnSendOtp.style.opacity = '1';
                        btnSendOtp.textContent = 'Lấy OTP';
                    }
                })
                .catch(err => {
                    console.error('Lỗi kết nối gửi OTP:', err);
                    alert('Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại!');
                    btnSendOtp.disabled = false;
                    btnSendOtp.style.opacity = '1';
                    btnSendOtp.textContent = 'Lấy OTP';
                });
        });
    }

    // ==========================================
    // 4. PREVIEW ANH DAI DIEN
    // ==========================================
    const avatarFile = document.getElementById('avatarFile');
    const avatarLabel = document.getElementById('avatarPreviewLabel');

    if (avatarFile && avatarLabel) {
        avatarFile.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    avatarLabel.innerHTML = `<img src="${event.target.result}" alt="Avatar Preview">`;
                };
                reader.readAsDataURL(file);
            }
        });
    }
});