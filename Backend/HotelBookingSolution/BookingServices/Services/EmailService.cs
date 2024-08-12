using MailKit.Net.Smtp;
using MimeKit;

namespace BookingServices.Services
{
    public class EmailService
    {
        private readonly HttpClient _httpClient;

        public EmailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendEmailWithImageUrlAsync(string toEmail, string subject, string body, string imageUrl)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Book My Stay", "huzkjoy@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            // Download the image from the URL
            var imageStream = await _httpClient.GetStreamAsync(imageUrl);
            var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var imageAttachment = new MimePart("image", "png")
            {
                Content = new MimeContent(memoryStream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "image.png"
            };

            bodyBuilder.Attachments.Add(imageAttachment);

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("huzkjoy@gmail.com", "Huz@lcu2002");

                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    throw new InvalidOperationException("Failed to send email.", ex);
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }
    }
}
