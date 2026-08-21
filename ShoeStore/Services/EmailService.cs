using System.Net;
using System.Net.Mail;

namespace ShoeStore.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            string smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
            int port = int.Parse(emailSettings["Port"] ?? "587");
            string senderEmail = emailSettings["SenderEmail"]!;
            string senderName = emailSettings["SenderName"] ?? "Minhee Sports";
            string appPassword = emailSettings["AppPassword"]!;

            var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = $"[{otpCode}] Mã xác thực OTP đăng ký - Minhee Sports",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f6f8;'>
                        <div style='max-width: 500px; margin: 0 auto; background: #ffffff; border-radius: 10px; padding: 25px; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #021156; text-align: center; margin-bottom: 20px;'>MINHEE SPORTS</h2>
                            <p>Xin chào,</p>
                            <p>Mã xác thực OTP để hoàn tất đăng ký tài khoản của bạn là:</p>
                            <div style='text-align: center; margin: 25px 0;'>
                                <span style='font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #203EDB; background: #EBF0FF; padding: 10px 25px; border-radius: 8px; display: inline-block;'>{otpCode}</span>
                            </div>
                            <p style='color: #718096; font-size: 13px;'>Mã này có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                            <hr style='border: none; border-top: 1px solid #edf2f7; margin: 20px 0;'>
                            <p style='text-align: center; color: #a0aec0; font-size: 12px;'>Cảm ơn bạn đã lựa chọn Minhee Sports!</p>
                        </div>
                    </div>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await client.SendMailAsync(mailMessage);
        }
    }
}
