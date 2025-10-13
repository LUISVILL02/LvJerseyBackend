using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Shared.Application.Abstractions;

namespace Shared.Infrastructure.Services;

public class SendGridEmailSender(IConfiguration config) : IEmailSender
{
    private readonly SendGridClient _client = new(config["SendGrid:ApiKey"]);
    private readonly string _from = new(config["SendGrid:From"]);
    private readonly string _name = new(config["SendGrid:Name"]);

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var from = new EmailAddress(_from, _name);
        var msg = MailHelper.CreateSingleEmail(from, new EmailAddress(to), subject, body, body);
        await _client.SendEmailAsync(msg);
    }
}