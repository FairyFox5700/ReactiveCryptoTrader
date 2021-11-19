using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CryptoTracker.BL.Utlis;
using H.Socket.IO;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.BL.Clients
{
    public class CryptoComparerClient
    {
        private readonly ILogger<CryptoComparerClient> _logger;

        public CryptoComparerClient(ILogger<CryptoComparerClient> logger)
        {
            _logger = logger;
        }

        public IObservable<Dictionary<string, object>> Connect(
            IObservable<string> input,
            IEnumerable<IMessageUnpacker> unpackers)
        {
            return Observable.Defer(() => Observable.Create<Dictionary<string, object>>(async (sink, token) =>
            {
                var taskCompletionSource = new TaskCompletionSource<dynamic>();
                var socket = new SocketIoClient();

                var connectionUri = new Uri("https://streamer.cryptocompare.com");
                _logger.LogInformation("[EXTERNAL-SERVICE] Connecting to CryptoCompare.com ...");

                Func<Task> closeSocket = async () =>
                {
                    taskCompletionSource.SetCanceled();
                    await socket.DisconnectAsync(token);
                    _logger.LogInformation("[EXTERNAL-SERVICE] Connection to CryptoCompare.com closed");
                };

                socket.Connected += (sender, args) =>
                {
                    input.Subscribe(v =>
                        {
                            string[] subscription = { v };
                            var subs = new Dictionary<string, object> { { "subs", subscription } };
                            socket.Emit("SubAdd", subs, cancellationToken: token);
                        },
                        onError: sink.OnError);
                };

                socket.On("m", async message =>
                {
                    var messageType = message[..message.IndexOf("~", StringComparison.Ordinal)];
                    foreach (var unpacker in unpackers)
                    {
                        if (!unpacker.Supports(messageType)) continue;

                        try
                        {
                            sink.OnNext(unpacker.Unpack(message));
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"ERROR: {e.Message}");
                            _logger.LogError($"{e.StackTrace}");
                            sink.OnError(e);
                            await closeSocket.Invoke();
                        }

                        break;
                    }
                });

                socket.ErrorReceived += (sender, args) => sink.OnError(new Exception(args.Value));
                socket.Disconnected += (sender, args) => sink.OnCompleted();

                token.Register(async () => await closeSocket());
                await socket.ConnectAsync(connectionUri, token);

                await taskCompletionSource.Task;
            }));
        }
    }
}