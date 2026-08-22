using System.Threading.Channels;

namespace Task4.Web.Services;

public record ConfirmationEmail(
    string Recipient,
    string ConfirmationUrl);

public class EmailQueue
{
    private readonly Channel<ConfirmationEmail> _queue =
        Channel.CreateUnbounded<ConfirmationEmail>();

    public ValueTask EnqueueAsync(
        ConfirmationEmail email,
        CancellationToken cancellationToken = default)
        => _queue.Writer.WriteAsync(email, cancellationToken);

    public ValueTask<ConfirmationEmail> DequeueAsync(
        CancellationToken cancellationToken = default)
        => _queue.Reader.ReadAsync(cancellationToken);
}