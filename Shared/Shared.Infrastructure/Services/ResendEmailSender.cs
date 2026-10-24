using Microsoft.Extensions.Configuration;
using Resend;
using Shared.Application.Abstractions;

namespace Shared.Infrastructure.Services;

public class ResendEmailSender(IResend resend, IConfiguration config) : IEmailSender
{
    private const string DefaultFromName = "LvJerseyAPP";
    private const string TestFromEmail = "onboarding@resend.dev";

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var fromEmail = config["Email:From"] 
            ?? config["SendGrid:From"] 
            ?? TestFromEmail;
        var fromName = config["Email:FromName"] 
            ?? config["SendGrid:Name"] 
            ?? DefaultFromName;

        var from = string.IsNullOrWhiteSpace(fromName) 
            ? fromEmail 
            : $"{fromName} <{fromEmail}>";

        var message = new EmailMessage
        {
            From = from,
            Subject = subject,
            HtmlBody = body
        };

        message.To.Add(to);

        await resend.EmailSendAsync(message);
    }
}
