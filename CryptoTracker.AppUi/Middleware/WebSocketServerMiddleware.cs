using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoTracker.BL.Sockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CryptoTracker.AppUi.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;
        private static readonly JsonSerializerSettings  SerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _manager.AddSocket(webSocket);
                var serviceScopeFactory = context.RequestServices.GetRequiredService<IServiceScopeFactory>();
                var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
             
                TradingWebSocketHandler wsHandler = serviceProvider.GetService<TradingWebSocketHandler>();
                await wsHandler.Handle(Observable.FromAsync(async () => await GetWebSocket(webSocket)))
                    .SelectMany(async (result) =>
                    {
                        byte[] output = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result, SerializerSettings));
                        await webSocket.SendAsync(new ArraySegment<byte>(output, 0, output.Length),
                            WebSocketMessageType.Text, true, CancellationToken.None);
                        return Unit.Default;
                    })
                    .LastOrDefaultAsync();
            }
            else
            {
#if DEBUG
                Debug.WriteLine("No WebSocket");
#endif
                await _next(context);
            }
        }
        private async Task<string> GetWebSocket(WebSocket webSocket)
        {
            var receivedMessage = String.Empty;
            await Receive(webSocket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    Console.WriteLine($"Receive->Text");
                    receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Message: {receivedMessage}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                    Console.WriteLine($"Receive->Close on: " + id);
    
                    WebSocket sock;
                    _manager.GetAllSockets().TryRemove(id, out sock);
                    Console.WriteLine("Managed Connections: " + _manager.GetAllSockets().Count.ToString());
    
                    await sock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                        CancellationToken.None);
                }
            });
            return receivedMessage;
        }
        
        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}