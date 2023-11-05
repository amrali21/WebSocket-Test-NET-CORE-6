using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSockets.Services
{
    public class SocketPool
    {
        public ConcurrentDictionary<string, WebSocket> ConnectionPool{ get; set; }
        
        public SocketPool()
        {
            ConnectionPool = new();
        }

        public void AddSocket(string key, WebSocket socket)
        {
            ConnectionPool.TryAdd(key, socket);
        }

        public void RemoveSocket(string key)
        {
            ConnectionPool.Remove(key, out _);
        }

        public void CleanPool()
        {
            foreach (KeyValuePair<string,WebSocket> pair in ConnectionPool)
            {
                if (pair.Value.State == WebSocketState.Closed)
                    ConnectionPool.Remove(pair.Key, out _);
            }
        }
    }
}
