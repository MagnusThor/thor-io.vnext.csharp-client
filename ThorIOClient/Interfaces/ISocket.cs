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
        ISocket OnConnect(Action<ISocket> onConnect);
        ISocket OnDisconnect(Action<ISocket> onDisconnect);
        ISocket OnMessage(Action<string, ISocket> onMessage);
        Task SendMessage(byte[] message);
        Task SendMessage(string message);
    }
}