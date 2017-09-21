using System;
using System.Collections.Generic;
using ThorIOClient.Interface;

namespace ThorIOClient
{
    public partial class Proxy
    {
        private ISerializer Serializer {get;set;}
        private WebSocketWrapper ws;
        public string alias;
        private List<Listener> Listeners;
        public Proxy(WebSocketWrapper ws, string alias,ISerializer serializer)
        {
        
            this.Serializer = serializer;
            this.Listeners = new List<Listener>();
            this.ws = ws;
            this.alias = alias;

            this.On<Models.ConnectionInfo>("___open", (Models.ConnectionInfo info) =>
            {
                this.OnOpen(info);
            });

            this.On<string>("___error", (string err) =>
            {
                this.OnError(err);
            });

        }
        public Action<Models.ConnectionInfo> OnOpen;
        public Action<Models.ConnectionInfo> OnClose;
        public Action<string> OnError;

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            this.Send(new Models.Message("___connect",
            "", this.alias));
        }
        private void Send(Models.Message message)
        {
            try
            {
                var data = Serializer.Serialize<Models.Message>(message);
                this.ws.SendMessage(data);
            }
            catch (Exception ex)
            {
                this.OnError(ex.Message);
            }

        }
        public Proxy Close()
        {
            this.Send(new Models.Message("___close", "", this.alias));
            return this;
        }
        public Proxy On<T>(string topic, Action<T> fn)
        {

            if (typeof(T) == typeof(IMessage))
            {
                this.Listeners.Add(new Listener(topic, message => fn((T)message)));
            }
            else
            {
                var listener = new Listener(topic, message => fn(Serializer.Deserialize<T>(message.Data)));
                this.Listeners.Add(listener);
            }


            return this;
        }
        public Proxy Off(string name)
        {
            var listener = this.FindListener(name);
            this.Listeners.Remove(listener);
            return this;

        }
        public Proxy Subscribe<T>(string topic, Action<T> fn)
        {

            var message = new Models.Message("___subscribe",
            Serializer.Serialize<Models.Subscription>(new Models.Subscription(topic, this.alias)), this.alias);

            this.Send(message);
            this.On<T>(topic, fn);

            return this;

        }

        public Proxy UnSubscribe(string topic)
        {
            var message = new Models.Message("___unsubscribe",
            Serializer.Serialize<Models.Subscription>(new Models.Subscription(topic, this.alias)), this.alias);
            this.Send(message);
            this.Off(topic);
            return this;
        }
        public Proxy Invoke<T>(string topic, T data)
        {
            this.Send(new Models.Message(topic, Serializer.Serialize<T>(data), this.alias));
            return this;
        }
        public Proxy Publish<T>(string topic, T data)
        {
            this.Invoke<T>(topic, data);
            return this;
        }

        public Proxy SetProperty<T>(string propName, T propValue)
        {
            this.Invoke<T>(propName, propValue);
            return this;
        }


        public void Dispatch(IMessage msg)
        {

            var listener = this.FindListener(msg.Topic);
            if (listener != null)
            {
                listener.fn(msg);
            }
            else
                return;


        }
        private Listener FindListener(string topic)
        {
            return this.Listeners.Find((Listener pre) =>
            {
                return pre.Topic == topic;
            });
        }
    }


}