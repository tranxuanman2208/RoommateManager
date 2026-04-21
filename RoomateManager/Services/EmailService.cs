using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;

namespace RoomateManager.Services
{
    public static class EmailService
    {
        // Thông tin cấu hình cố định
        private const string Host = "smtp.gmail.com";
        private const string FromEmail = "tranman22082006@gmail.com";
        private const string AppPassword = "tzfa jmrv zcof jtjb";
        public static async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient(Host)
                {
                    Port = 587,
                    Credentials = new NetworkCredential(FromEmail, AppPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(FromEmail, "Roommate Manager System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Để true nếu bạn muốn nội dung có định dạng HTML
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi Email: " + ex.Message);
                return false;
            }
        }
    }
}
