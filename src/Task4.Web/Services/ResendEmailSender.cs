using Resend;

namespace Task4.Web.Services;

public class ResendEmailSender(IResend resend) : IEmailSender
{
    public async Task SendConfirmationEmailAsync(
        string recipient,
        string confirmationUrl,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = "onboarding@resend.dev",
            Subject = "Confirm your Task4 account",
            HtmlBody = $"""
                <h2>Confirm your email</h2>
                <p>Click the button below to confirm your email address.</p>
                <p>
                    <a href="{confirmationUrl}">
                        Confirm email
                    </a>
                </p>
                """
        };

        message.To.Add(recipient);

        await resend.EmailSendAsync(message);
    }
}