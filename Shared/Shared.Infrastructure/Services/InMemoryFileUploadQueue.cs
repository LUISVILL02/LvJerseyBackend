using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions;
using Shared.Application.Dtos;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Implementación de cola en memoria usando System.Threading.Channels.
/// Los mensajes se pierden si la aplicación se reinicia.
/// </summary>
public sealed class InMemoryFileUploadQueue : IFileUploadQueue
{
    private readonly Channel<FileUploadMessage> _channel;
    private readonly ILogger<InMemoryFileUploadQueue> _logger;
    private int _count;

    public InMemoryFileUploadQueue(ILogger<InMemoryFileUploadQueue> logger)
    {
        _logger = logger;

        // Canal sin límite - no bloquea al productor
        var options = new UnboundedChannelOptions
        {
            SingleReader = false, // Permitir múltiples lectores (aunque usaremos uno)
            SingleWriter = false  // Permitir múltiples escritores (handlers concurrentes)
        };

        _channel = Channel.CreateUnbounded<FileUploadMessage>(options);

        _logger.LogInformation("InMemoryFileUploadQueue inicializado");
    }

    public int Count => _count;

    public async ValueTask EnqueueAsync(
        FileUploadMessage message,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
        Interlocked.Increment(ref _count);

        _logger.LogDebug(
            "Mensaje encolado para archivo {FileId}. Total en cola: {Count}",
            message.IdFile, _count);
    }

    public async IAsyncEnumerable<FileUploadMessage> DequeueAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            Interlocked.Decrement(ref _count);

            _logger.LogDebug(
                "Mensaje desencolado para archivo {FileId}. Restantes en cola: {Count}",
                message.IdFile, _count);

            yield return message;
        }
    }
}
