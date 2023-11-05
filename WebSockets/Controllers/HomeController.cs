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
        private readonly ILogger<HomeController> _logger;
        private string currentStatus = string.Empty;

        private static ConcurrentDictionary<string, WebSocket> connectionPoolTest { get; set; }

        WebSocket socket;
        SocketPool _socketPool;

        public HomeController(ILogger<HomeController> logger, SocketPool socketPool)
        {
            _logger = logger;
            _socketPool = socketPool;

            if (connectionPoolTest == null)
                connectionPoolTest = new();
        }

        public IActionResult Index()
        {
            return View();
        }

        //[Route("/ws")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/ws")]
        public async Task GetSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                socket = webSocket;
                //await UpdateStatus();
                //await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Route("/ws2")]
        public async Task GetSocket2(/*HttpContext context*/)
        {
            //var test = context.Request.Query["socketId"];

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string key = HttpContext.Request.Query["socketId"];
                socket = webSocket;
                //await sendMessage_old("a7aaaaaaaaaa");

                _socketPool.AddSocket(key, webSocket);
                //connectionPoolTest.TryAdd(key, webSocket);

                var buffer = new byte[1024 * 4];
                try
                {
                    var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                    while (!receiveResult.CloseStatus.HasValue)
                    {
                        receiveResult = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                }

                catch (Exception ex)
                {

                }

                //await UpdateStatus(key);

                //await UpdateStatus();
                //await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task sendMessage_old(string message)
        {
            if (socket.State != WebSocketState.Open)
            {

            }
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task sendMessage(WebSocket socket, string message)
        {
            if(socket.State != WebSocketState.Open)
            {

            }
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(string socketId)
        {
            for(int i = 0; i < 100; i++)
            {
                await Task.Delay(1000);
                try
                {
                    await sendMessage(_socketPool.ConnectionPool[socketId], $"hello world {i}");
                } catch(Exception ex)
                {

                }
            }
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}