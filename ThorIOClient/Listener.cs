using System;
using ThorIOClient;
using ThorIOClient.Interfaces;

public partial class Listener{

        public Action<IMessage> fn;
        public string Topic;

        public Type Type { get; private set; }

        public Listener(string topic, Action<IMessage> fn){
            this.Topic = topic;
            this.fn = fn;
        }
    }
