using System;
using ThorIOClient.Interfaces;

namespace ThorIOClient.Models
{

    public class NetworkEventMessage<T> : INetworkMessageEvent
    {
        public T value { get; set; }
        public float ts { get; set; }

        public NetworkEventMessage(T data)
        {
            value = data;
            ts = DateTime.Now.Ticks;
        }

    }
}