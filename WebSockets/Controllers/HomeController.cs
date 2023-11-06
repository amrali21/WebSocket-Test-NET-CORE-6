using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using WebSockets.Models;
using WebSockets.Services;

namespace WebSockets.Controllers
{
    public class HomeController : Controller
    {
        SocketPool _socketPool;

        public HomeController( SocketPool socketPool)
        {
            _socketPool = socketPool;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/ws2")]
        public async Task GetSocket2()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string key = HttpContext.Request.Query["socketId"];
                string user = $"user {_socketPool.ConnectionPool.Count + 1}";

                _socketPool.AddSocket(key, webSocket);

                _socketPool.Broadcast(user, "joined chat");

                try
                {
                    var buffer = new byte[1024 * 4];
                    var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                    while (!receiveResult.CloseStatus.HasValue)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

                        _socketPool.Broadcast(user, message);

                        receiveResult = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), CancellationToken.None);
                    }

                    await webSocket.CloseAsync(
                        receiveResult.CloseStatus.Value,
                        receiveResult.CloseStatusDescription,
                        CancellationToken.None);

                    _socketPool.RemoveSocket(key);
                }
                catch {}
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    }
}