using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Task4.Web.Services;

public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendConfirmationEmailAsync(
        string recipient,
        string confirmationUrl,
        CancellationToken cancellationToken = default)
    {
        var host =
            configuration["Smtp:Host"]
            ?? throw new InvalidOperationException("Smtp:Host is not configured.");

        var port =
            int.Parse(configuration["Smtp:Port"] ?? "587");

        var username =
            configuration["Smtp:Username"]
            ?? throw new InvalidOperationException("Smtp:Username is not configured.");

        var password =
            configuration["Smtp:Password"]
            ?? throw new InvalidOperationException("Smtp:Password is not configured.");

        var fromAddress =
            configuration["Smtp:FromAddress"] ?? username;

        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(fromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = "Confirm your Task4 account";

        message.Body = new BodyBuilder
        {
            HtmlBody = $"""
                <h2>Confirm your email</h2>
                <p>Click the button below to confirm your email address.</p>
                <p>
                    <a href="{confirmationUrl}">
                        Confirm email
                    </a>
                </p>
                """
        }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}