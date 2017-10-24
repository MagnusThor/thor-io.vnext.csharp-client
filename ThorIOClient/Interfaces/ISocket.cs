using System;
using System.Threading.Tasks;
using ThorIOClient.Queue;

namespace ThorIOClient.Interfaces
{
    public interface ISocket
    {
        ThreadSafeQueue<byte[]> Queue { get; set; }
        Task<ISocket> Close();
        Task<ISocket> Connect();
        Task ConnectAsync();
        ISocket OnConnect(Action<ISocket> onConnect);
        ISocket OnDisconnect(Action<ISocket> onDisconnect);
        ISocket OnMessage(Action<string, ISocket> onMessage);
        Task SendMessage(byte[] message);
        Task SendMessage(string message);
        Task SendMessageAsync(byte[] messageBuffer);
        Task StartListen();
    }
}