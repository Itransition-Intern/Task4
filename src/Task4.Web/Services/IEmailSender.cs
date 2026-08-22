namespace Task4.Web.Services;

public interface IEmailSender
{
    Task SendConfirmationEmailAsync(
        string recipient,
        string confirmationUrl,
        CancellationToken cancellationToken = default);
}