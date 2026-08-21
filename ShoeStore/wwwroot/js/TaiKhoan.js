document.addEventListener('DOMContentLoaded', () => {

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

    const otpInputs = document.querySelectorAll('.otp-input');
    const fullOtpInput = document.getElementById('fullOtpCode');
    const otpStatusMsg = document.getElementById('otpStatusMsg');

    function getActiveEmail() {
        const regEmail = document.getElementById('regEmail');
        const resetEmail = document.getElementById('resetEmail');
        if (regEmail && regEmail.value.trim() !== '') return regEmail.value.trim();
        if (resetEmail && resetEmail.value.trim() !== '') return resetEmail.value.trim();
        return '';
    }

    if (otpInputs.length > 0) {
        otpInputs.forEach((input, index) => {

            input.addEventListener('input', (e) => {
                const val = e.target.value;

                if (!/^\d*$/.test(val)) {
                    e.target.value = '';
                    return;
                }

                if (val.length === 1 && index < otpInputs.length - 1) {
                    otpInputs[index + 1].focus();
                }

                updateFullOtpAndCheck();
            });

            input.addEventListener('keydown', (e) => {
                if (e.key === 'Backspace' && !input.value && index > 0) {
                    otpInputs[index - 1].focus();
                }
            });
        });

        updateFullOtpAndCheck();

        function updateFullOtpAndCheck() {
            let otpStr = '';
            otpInputs.forEach(inp => otpStr += inp.value);
            if (fullOtpInput) fullOtpInput.value = otpStr;

            if (otpStr.length === 6) {
                const email = getActiveEmail();

                if (!email) {
                    if (otpStatusMsg) {
                        otpStatusMsg.style.color = '#FF8A8A';
                        otpStatusMsg.textContent = 'Vui lòng nhập Email trước khi kiểm tra OTP!';
                    }
                    return;
                }

                if (otpStatusMsg) {
                    otpStatusMsg.style.color = '#FCB90D';
                    otpStatusMsg.textContent = 'Đang kiểm tra mã OTP...';
                }

                fetch(`/TaiKhoan/KiemTraOTP?email=${encodeURIComponent(email)}&otpCode=${encodeURIComponent(otpStr)}`, {
                    method: 'POST'
                })
                    .then(res => res.json())
                    .then(data => {
                        if (otpStatusMsg) {
                            if (data.success) {
                                otpStatusMsg.style.color = '#10B981';
                                otpStatusMsg.textContent = '✓ ' + (data.message || 'Mã OTP chính xác!');
                            } else {
                                otpStatusMsg.style.color = '#FF8A8A';
                                otpStatusMsg.textContent = '✕ ' + (data.message || 'Mã OTP không chính xác!');
                            }
                        }
                    })
                    .catch(err => {
                        console.error('Lỗi kiểm tra OTP:', err);
                        if (otpStatusMsg) {
                            otpStatusMsg.style.color = '#FF8A8A';
                            otpStatusMsg.textContent = 'Lỗi kết nối kiểm tra OTP!';
                        }
                    });
            } else {
                if (otpStatusMsg) otpStatusMsg.textContent = '';
            }
        }
    }

    const btnSendOtp = document.getElementById('btnSendOtp');
    const regEmailInput = document.getElementById('regEmail');

    if (btnSendOtp) {
        btnSendOtp.addEventListener('click', () => {
            const email = regEmailInput ? regEmailInput.value.trim() : '';
            if (!email) {
                alert('Vui lòng nhập địa chỉ Email trước khi lấy mã OTP!');
                if (regEmailInput) regEmailInput.focus();
                return;
            }

            btnSendOtp.disabled = true;
            btnSendOtp.style.opacity = '0.6';
            btnSendOtp.textContent = 'Đang gửi...';

            fetch(`/TaiKhoan/GuiOTP?email=${encodeURIComponent(email)}`, { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message || 'Mã OTP đã được gửi về Email của bạn!');

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

    const btnSendOtpReset = document.getElementById('btnSendOtpReset');
    const resetEmailInput = document.getElementById('resetEmail');

    if (btnSendOtpReset) {
        btnSendOtpReset.addEventListener('click', () => {
            const email = resetEmailInput ? resetEmailInput.value.trim() : '';
            if (!email) {
                alert('Vui lòng nhập Email trước khi lấy mã OTP!');
                if (resetEmailInput) resetEmailInput.focus();
                return;
            }

            btnSendOtpReset.disabled = true;
            btnSendOtpReset.style.opacity = '0.6';
            btnSendOtpReset.textContent = 'Đang gửi...';

            fetch(`/TaiKhoan/GuiOTPQuenMatKhau?email=${encodeURIComponent(email)}`, { method: 'POST' })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message || 'Mã OTP đã được gửi về Email của bạn!');

                        let countdown = 60;
                        const timer = setInterval(() => {
                            btnSendOtpReset.textContent = `Thử lại (${countdown}s)`;
                            countdown--;

                            if (countdown < 0) {
                                clearInterval(timer);
                                btnSendOtpReset.disabled = false;
                                btnSendOtpReset.style.opacity = '1';
                                btnSendOtpReset.textContent = 'Lấy OTP';
                            }
                        }, 1000);
                    } else {
                        alert(data.message || 'Lỗi gửi mã OTP!');
                        btnSendOtpReset.disabled = false;
                        btnSendOtpReset.style.opacity = '1';
                        btnSendOtpReset.textContent = 'Lấy OTP';
                    }
                })
                .catch(err => {
                    console.error('Lỗi kết nối:', err);
                    alert('Không thể kết nối đến máy chủ!');
                    btnSendOtpReset.disabled = false;
                    btnSendOtpReset.style.opacity = '1';
                    btnSendOtpReset.textContent = 'Lấy OTP';
                });
        });
    }

    const btnToggleProfile = document.getElementById('btnToggleProfile');
    const editableFields = document.querySelectorAll('.editable-field');
    const inputAvatar = document.getElementById('inputAvatar');
    const avatarEditBadge = document.getElementById('avatarEditBadge');
    const profileForm = document.getElementById('profileForm');
    const avatarPreview = document.getElementById('avatarPreview');

    if (btnToggleProfile) {
        let isEditing = false; 

        btnToggleProfile.addEventListener('click', () => {
            if (!isEditing) {
                
                isEditing = true;

                editableFields.forEach(field => field.disabled = false);
                if (inputAvatar) inputAvatar.disabled = false;
                if (avatarEditBadge) avatarEditBadge.style.display = 'flex';

                btnToggleProfile.textContent = 'XÁC NHẬN CẬP NHẬT';
                btnToggleProfile.classList.remove('btn-edit-mode');
                btnToggleProfile.classList.add('btn-save-mode');

                if (editableFields.length > 0) editableFields[0].focus();
            } else {
                
                if (profileForm) profileForm.submit();
            }
        });
    }

    if (inputAvatar && avatarPreview) {
        inputAvatar.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    avatarPreview.src = event.target.result;
                };
                reader.readAsDataURL(file);
            }
        });
    }

});
