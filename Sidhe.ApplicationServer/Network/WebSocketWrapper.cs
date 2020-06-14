using System;

using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;

namespace Sidhe.ApplicationServer.Network
{
    public class WebSocketWrapper : IWebSocket
    {
        [NotNull] private readonly WebSocket _socket;


        public WebSocketWrapper([NotNull] WebSocket socket)
        {
            _socket = socket;
        }

        
        public Task Close(WebSocketCloseStatus status, string description, CancellationToken cancellationToken)
            => _socket.CloseAsync(status, description, cancellationToken);


        public void Dispose() => _socket.Dispose();


        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            => _socket.ReceiveAsync(buffer, cancellationToken);


        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType type, bool isEndOfMessage, CancellationToken cancellationToken) 
            => _socket.SendAsync(buffer, type, isEndOfMessage, cancellationToken);


        public WebSocketState State => _socket.State;
    }
}
