using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;

namespace CryptoTracker.AppUi.Middleware
{
    public class WebSocketServerConnectionManager
    {
       
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public void AddSocket(WebSocket socket)
        {
            string connId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connId, socket);
#if DEBUG
            Debug.WriteLine("WebSocketServerConnectionManager-> AddSocket: WebSocket added with ID: " + connId);
#endif
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }
    }
}