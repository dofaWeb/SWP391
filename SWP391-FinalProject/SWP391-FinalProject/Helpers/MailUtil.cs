using System.Net.Mail;
using System.Net;
using Humanizer;
using System.Runtime.InteropServices.JavaScript;
using SWP391_FinalProject.Models;
using SWP391_FinalProject.Repository;

namespace SWP391_FinalProject.Helpers
{
    public class MailUtil
    {

        public static async Task SendBillEmail(OrderModel order, string username, decimal? totalPrice, int Point, List<ProductItemModel> listProItem)
        {
            string _from = "meocho0432004@gmail.com";
            string _password = "pnrc uxpz dlxw pjqp";
            string _subject = "Electronic Shop Bill";
            UserRepository userRepo = new UserRepository();
            UserModel user = userRepo.GetUserProfileByUsername(username);
            string _to = user.Account.Email;
            // Cấu trúc email body với bảng HTML
            string _body = "<h3>Order Information</h3>"
                            + "<p><strong>Name:</strong> " + user.Name + "</p>"
                            + "<p><strong>Address:</strong> " + order.Addres+ "</p>"
                            + "<p><strong>State:</strong> Pending</p>"
                            + "<p><strong>Date:</strong> " + DateTime.Now.TimeOfDay + "</p>"
                            + "<p><strong>Use Point:</strong> " + order.UsePoint + "</p>"
                            + "<p><strong>Earn Point:</strong> " + ((totalPrice ?? 0) / 1000).ToString("N0") + "</p>"
                            + "<h4>Product Items</h4>"
                            + "<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse:collapse;'>"
                            + "<thead>"
                            + "<tr>"
                            + "<th>Product Name</th>"
                            + "<th>Ram</th>"
                            + "<th>Storage</th>"
                            + "<th>Quantity</th>"
                            + "<th>Price(VND)</th>"
                            + "</tr>"
                            + "</thead>"
                            + "<tbody>";

            foreach (var item in listProItem)
            {
                ProductRepository proRepo = new ProductRepository();
                ProductModel product = proRepo.GetProductById(item.Product.Id);

                _body += "<tr>"
                        + "<td>" + product.Name + "</td>"
                        + "<td>" + item.Ram + "</td>"
                        + "<td>" + item.Storage + "</td>"
                        + "<td>" + item.CartQuantity + "</td>"
                        + "<td>" + item.PriceAfterDiscount?.ToString("N0") + "</td>"
                        + "</tr>";
            }

            _body += "</tbody></table>";
            _body += "<h4>Total Price: <b>" + (totalPrice - Point)?.ToString("N0") + "</b> VND</h4>";
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

        public static async Task SendRegisterStaffEmail(string _to, string username, string password)
        {
            string _from = "meocho0432004@gmail.com";
            string _password = "pnrc uxpz dlxw pjqp";
            string _subject = "Electronic Shop Register Staff Account Email";
            string _body = "<div style='font-family:Arial,sans-serif;font-size:14px;color:#333;'>"
                + "<p>This is your username: <b><em>" + username + "</em></b></p>"
                + "<p>This is your password: <b><em>" + password + "</em></b></p>"
                + "<p>You can use this to login as a staff</p>"
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
