using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IWebSocket : IDisposable
    {
        [NotNull] Task Close(WebSocketCloseStatus status, string description, CancellationToken cancellationToken);
        [NotNull] Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        [NotNull] Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType type, bool isEndOfMessage, CancellationToken cancellationToken);

        WebSocketState State { get; }
    }
}