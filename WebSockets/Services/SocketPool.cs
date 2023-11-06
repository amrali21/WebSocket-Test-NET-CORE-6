using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

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

        public async void Broadcast(string user, string message)
        {
            foreach (KeyValuePair<string, WebSocket> pair in ConnectionPool)
            {
                string messageText = $"{user}: {message}";
                await sendMessage(pair.Value, messageText);
            }
        }

        private async Task sendMessage(WebSocket socket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
