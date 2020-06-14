using System;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities;

namespace Sidhe.ApplicationServer.Network
{
    public class WebSocketClient : IApplicationClient
    {
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const int InitialiseAuthenticateSize = 40;
        private const int InitialiseTimeoutSeconds = 5;


        [NotNull] private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        [NotNull] private readonly object _lock = new object();
        [NotNull] private readonly byte[] _readBuffer;
        [NotNull] private readonly IWebSocket _socket;
        [NotNull] private readonly byte[] _writeBuffer;
        private Action<IApplicationClient> _onClose;


        public WebSocketClient([NotNull] IWebSocket socket, [NotNull] IApplicationSettings settings)
        {
            _readBuffer = new byte[Math.Max(settings.ClientReceiveBytes, InitialiseAuthenticateSize)];
            _writeBuffer = new byte[settings.ClientSendBytes];
            _socket = socket;
        }


        public string LoginId { get; private set; }
        public int UserId { get; private set; }


        private void Close()
        {
            if (_socket.State == WebSocketState.Open)
            {
                try
                {
                    _onClose?.Invoke(this);

                    var writeLength = Encoding.ASCII.GetBytes("QUIT", new Span<byte>(_writeBuffer));

                    _socket.SendAsync(
                        new ArraySegment<byte>(_writeBuffer, 0, writeLength), 
                        WebSocketMessageType.Text,true, CancellationToken.None)
                        .Wait(TimeSpan.FromSeconds(1));

                    _socket
                        .Close(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None)
                        .Wait(TimeSpan.FromSeconds(1));
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception, $"Error on {GetType().Name} disposal.");
                }
            }
        }
    

        public void Dispose()
        {
            _socket.Dispose();
        }


        public async Task<bool> Initialise(Action<IApplicationClient> onClose)
        {
            try
            {
                lock (_lock)
                {
                    if (_onClose != null)
                    {
                        throw new InvalidOperationException("Client is already initialised.");
                    }

                    _onClose = onClose;
                }

                var finishTime = DateTime.UtcNow.AddSeconds(InitialiseTimeoutSeconds);
                var readPosition = 0;

                TimeSpan timeLeft;

                do
                {
                    timeLeft = finishTime - DateTime.UtcNow;

                    if (timeLeft < TimeSpan.Zero)
                    {
                        timeLeft = TimeSpan.Zero;
                    }

                    var readTask = _socket.ReceiveAsync(
                        new ArraySegment<byte>(_readBuffer, readPosition, InitialiseAuthenticateSize - readPosition),
                        CancellationToken.None);

                    if (readTask.Wait(timeLeft))
                    {
                        var readResult = await readTask;

                        if (readResult.CloseStatus == null)
                        {
                            readPosition += readResult.Count;

                            if (readResult.EndOfMessage)
                            {
                                // The message format should be: 4 bytes of user ID, 12 bytes of session ID.
                                if (readResult.MessageType == WebSocketMessageType.Text && readPosition == InitialiseAuthenticateSize &&
                                    int.TryParse(Encoding.ASCII.GetString(_readBuffer, 0, 8), NumberStyles.AllowHexSpecifier, null, out var userId))
                                {
                                    var loginId = Encoding.ASCII.GetString(_readBuffer, 8, InitialiseAuthenticateSize - 8);

                                    if (loginId.IsHexadecimal()                                        )
                                    {
                                        LoginId = loginId;
                                        UserId = userId;

                                        return true;
                                    }
                                }

                                break;
                            }
                        }
                    }
                } while (readPosition < InitialiseAuthenticateSize && timeLeft > TimeSpan.Zero && _socket.State == WebSocketState.Open);

                Close();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Exception while initialising client from socket.");
            }

            return false;
        }

        
        public async Task Run(Func<ClientRequest, Session, ClientResponse> dispatch, Session session)
        {
            var writeLength = Encoding.ASCII.GetBytes("OPEN", new Span<byte>(_writeBuffer));

            await _socket.SendAsync(
                new ArraySegment<byte>(_writeBuffer, 0, writeLength), 
                WebSocketMessageType.Text,
                true, CancellationToken.None);

            while (!_cancellation.IsCancellationRequested)
            {
                var readPosition = 0;

                try
                {
                    while (!_cancellation.IsCancellationRequested)
                    {
                        var readResult = await _socket.ReceiveAsync(
                            new ArraySegment<byte>(_readBuffer, readPosition, _readBuffer.Length - readPosition),
                            _cancellation.Token);

                        if (readResult.CloseStatus != null)
                        {
                            _cancellation.Cancel();
                            break;
                        }

                        readPosition += readResult.Count;

                        if (readResult.EndOfMessage)
                        {
                            if (readResult.MessageType == WebSocketMessageType.Text)
                            {
                                var json = Encoding.UTF8.GetString(_readBuffer, 0, readPosition);
                                var request = JsonConvert.DeserializeObject<ClientRequest>(json);

                                var response = dispatch(request, session);

                                if (response.HasContents())
                                {
                                    await _socket.SendAsync(
                                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))),
                                        WebSocketMessageType.Text, true, _cancellation.Token);
                                }
                            }
                            else
                            {
                                _cancellation.Cancel();
                            }
                            break;
                        }

                        if (readPosition == _readBuffer.Length)
                        {
                            _cancellation.Cancel();
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    break;
                }
            }

            Close();
        }


        public void Terminate()
        {
            lock (_lock)
            {
                 Close();
                _cancellation.Cancel();
            }
        }
    }
}
