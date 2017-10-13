using System;
using System.Threading.Tasks;
using ThorIOClient.Queue;

namespace ThorIOClient.Models
{
    public interface ISocket
    {
        ThreadSafeQueue<byte[]> Queue { get; set; }
        Task<WebSocketWrapper> Close();
        Task<WebSocketWrapper> Connect();
        WebSocketWrapper OnConnect(Action<WebSocketWrapper> onConnect);
        WebSocketWrapper OnDisconnect(Action<WebSocketWrapper> onDisconnect);
        WebSocketWrapper OnMessage(Action<string, WebSocketWrapper> onMessage);
        Task SendMessage(byte[] message);
        Task SendMessage(string message);
    }
}