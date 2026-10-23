using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.DTOs.Email;
using BaseAccountSystem.Server.Interfaces.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BaseAccountSystem.Server.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _smtpSettings = options.Value;
        }

        public async Task CreateEmail(NewEmailDto dto)
        {
            if (string.IsNullOrEmpty(dto.ToEmail)) return;

            var message = new MimeMessage();

            var from = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
            message.From.Add(from);

            var to = new MailboxAddress(dto.ToUser ?? "", dto.ToEmail);
            message.To.Add(to);

            message.Subject = dto.Subject ?? "System email";

            var bb = new BodyBuilder();
            bb.TextBody = dto.HtmlBody ?? "This email was sent without a body.";
            bb.HtmlBody = dto.HtmlBody ?? "This email was sent without a body.";

            message.Body = bb.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port);

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
