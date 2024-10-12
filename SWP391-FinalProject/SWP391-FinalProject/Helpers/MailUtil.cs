using System.Net.Mail;
using System.Net;
using Humanizer;
using System.Runtime.InteropServices.JavaScript;

namespace SWP391_FinalProject.Helpers
{
    public class MailUtil
    {
        public static async Task SendRegisterEmail(string _to)
        {
            string _from = "meocho0432004@gmail.com";
            string _password = "pnrc uxpz dlxw pjqp";
            string _subject = "Electronic Shop Register Account";
            string _body = "Click here to active your account: http://localhost:5067/Acc/ReceiveRegisterEmail";
            MailMessage message = new MailMessage(_from, _to, _subject, _body);
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            message.ReplyToList.Add(new MailAddress(_from));
            message.Sender = new MailAddress(_from);

            using var smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(_from, _password);

            // Sử dụng bất đồng bộ với await để gửi email
            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
            }

        }

        public static async Task SendForgetPasswordEmail(string _to, int Otp)
        {
            string _from = "meocho0432004@gmail.com";
            string _password = "pnrc uxpz dlxw pjqp";
            string _subject = "Electronic Shop Forget Password Email";
            string _body = "<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;'>"
                + "<p>This is your OTP code: <b><em>" + Otp + "</em></b></p>"
                + "<p>Please copy this code and enter it to reset your account password.</p>"
                + "<p>If you did not request a password reset, please ignore this email.</p>"
                + "</div>";
            MailMessage message = new MailMessage(_from, _to, _subject, _body);
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            message.ReplyToList.Add(new MailAddress(_from));
            message.Sender = new MailAddress(_from);

            using var smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(_from, _password);

            // Sử dụng bất đồng bộ với await để gửi email
            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
            }

        }
        public static async Task<string> SendGmailAsync(string _from, string _password, string _to, string _subject, string _body)
        {
            MailMessage message = new MailMessage(_from, _to, _subject, _body);
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;

            message.ReplyToList.Add(new MailAddress(_from));
            message.Sender = new MailAddress(_from);

            using var smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(_from, _password);

            // Sử dụng bất đồng bộ với await để gửi email
            try
            {
                await smtpClient.SendMailAsync(message);
                return "Success";
            }
            catch (Exception ex)
            {
                return "Fail " + ex.Message;
            }

        }
    }
}
