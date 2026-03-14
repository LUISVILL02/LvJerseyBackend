using Shared.Application.Dtos;

namespace Shared.Application.Abstractions;

/// <summary>
/// Interface para cola de mensajes de subida de archivos.
/// Abstrae el mecanismo de cola (in-memory, RabbitMQ, etc.)
/// </summary>
public interface IFileUploadQueue
{
    /// <summary>
    /// Encola un mensaje para procesamiento asíncrono.
    /// </summary>
    /// <param name="message">Mensaje con información del archivo a subir</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    ValueTask EnqueueAsync(FileUploadMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desencola todos los mensajes disponibles como un stream asíncrono.
    /// Este método bloquea esperando nuevos mensajes hasta que se cancela.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Stream asíncrono de mensajes</returns>
    IAsyncEnumerable<FileUploadMessage> DequeueAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene el número aproximado de mensajes en la cola.
    /// </summary>
    int Count { get; }
}
