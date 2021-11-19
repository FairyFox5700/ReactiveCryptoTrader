using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CryptoTracker.BL.Utlis;
using SocketIOClient;
using SocketIOClient.Transport;

namespace CryptoTracker.ConsoleApp
{
    class Program
    {
        private const string StreamerCryptocompareUrl = "https://streamer.cryptocompare.com";
        static void Main(string[] args)
        {
            var unpackers = new List<IMessageUnpacker>()
            {
                new PriceMessageUnpacker(),
                new TradeMessageUnpacker(),
            };
            var list =new List<string>()
            {
                "5~CCCAGG~BTC~USD", "0~Coinbase~BTC~USD", "0~Cexio~BTC~USD",
            };
            var input = list.ToObservable();
            Observable.Defer(()=>Observable.Create<Dictionary<string, object>>(async (sink, token) =>
                {
                    var taskCompletionSource = new TaskCompletionSource<dynamic>();
                    Uri connectionUri = new Uri("https://streamer.cryptocompare.com");
                    var socket = new SocketIO(connectionUri, new SocketIOOptions()
                    {
                        Transport = TransportProtocol.WebSocket,
                        EIO = 3, //do not delete
                    });

                    // await socket.ConnectAsync(connectionUri);
                    //_logger.LogInformation("[EXTERNAL-SERVICE] Connecting to CryptoCompare.com ...");

                    Func<Task> closeSocket = async () =>
                    {
                        taskCompletionSource.SetCanceled();
                        await socket.DisconnectAsync();
                        //_logger.LogInformation("[EXTERNAL-SERVICE] Connection to CryptoCompare.com closed");
                    };

                    socket.OnConnected += (sender, args) =>
                    {
                        input.Subscribe(async v =>
                            {
                                string[] subscription = {v};
                                Dictionary<string, object> subs = new Dictionary<string, object>();
                                subs.Add("subs", subscription);
                                await socket.EmitAsync("SubAdd", subs);
                            },
                            onError: e => sink.OnError(e));
                    };

                    socket.On("m", async args =>
                    {
                        string message = args.ToString();
                        string messageType = message.Substring(0, message.IndexOf("~"));
                        foreach (IMessageUnpacker unpacker in unpackers)
                        {
                            if (unpacker.Supports(messageType))
                            {
                                try
                                {
                                    sink.OnNext(unpacker.Unpack(message));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"ERROR: {e.Message}");
                                    Console.WriteLine($"{e.StackTrace}");
                                    sink.OnError(e);
                                    await closeSocket.Invoke();
                                }

                                break;
                            }
                        }
                    });

                    socket.OnError += (sender, args) => sink.OnError(new Exception(args));
                    socket.OnDisconnected += (sender, args) => sink.OnCompleted();

                    token.Register(async () => await closeSocket());
                    await socket.ConnectAsync();

                    await taskCompletionSource.Task;
                }))
                .SelectMany(e => e.Keys)
                .Do(e => Console.WriteLine(e))
                .Subscribe();
           //["5~CCCAGG~BTC~USD~1~67944.53~1636440228~67952~0.02816695~1914.297466053~233647972~10190.654423831502~691736789.0696819~33957.86728353999~2265900457.849409~67549.84~68514.26~67144.21~65958.46~68532.93~65199.24~Coinbase~817.8873594799912~55595083.72647147~68139.38~68206.26~67740.93~33957.78138253999~2265894822.9477787~3fffff9"]
                Console.ReadLine();
        }
    }
}