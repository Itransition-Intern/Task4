namespace Task4.Web.Services;

public class EmailBackgroundService(
    EmailQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (var email in ReadQueue(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var emailSender =
                    scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await emailSender.SendConfirmationEmailAsync(
                    email.Recipient,
                    email.ConfirmationUrl,
                    stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to send confirmation email to {Recipient}.",
                    email.Recipient);
            }
        }
    }

    private async IAsyncEnumerable<ConfirmationEmail> ReadQueue(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return await queue.DequeueAsync(cancellationToken);
        }
    }
}