using System;
using System.Collections.Generic;


namespace ThorIOClient
{
    public class ConnectionInfo
    {
            public string CI;
            public string C;
            public string TS;
            public ConnectionInfo(){}
    }
    public partial class Proxy
    {
        private WebSocketWrapper ws;
        public string alias;
        private List<Listener> Listeners;
        public Proxy(WebSocketWrapper ws, string alias)
        {
            this.Listeners = new List<Listener>();
            this.ws = ws;
            this.alias = alias;

            this.On<ConnectionInfo>("___open", (ConnectionInfo info) =>
            {
                this.OnOpen(info);
            });

            this.On<string>("___error", (string err) =>
            {
                this.OnError(err);
            });

        }
        public Action<ConnectionInfo> OnOpen;
        public Action<ConnectionInfo> OnClose;
        public Action<string> OnError;

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            this.Send(new ThorIOClient.Message("___connect",
            "", this.alias));
        }
        private void Send(ThorIOClient.Message message)
        {
            var data = JsonHelper.JsonSerializer<ThorIOClient.Message>(message);
            this.ws.SendMessage(data);
        }
        public Proxy Close()
        {
            this.Send(new ThorIOClient.Message("___close", "", this.alias));
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
                var listener = new Listener(topic, message => fn(JsonHelper.JsonDeserialize<T>(message.Data)));
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
            var subscription = JsonHelper.JsonSerializer<SubscriptionModel>(new SubscriptionModel(topic, this.alias));
            this.Send(new ThorIOClient.Message("___subscribe", subscription, this.alias));
            this.On<T>(topic, fn);
            return this;

        }

        public Proxy UnSubscribe(string topic)
        {
            var subscription = JsonHelper.JsonSerializer<SubscriptionModel>(new SubscriptionModel(topic, this.alias));
            this.Send(new ThorIOClient.Message("___unsubscribe", subscription, this.alias));
            this.Off(topic);
            return this;
        }
        public Proxy Invoke<T>(string topic, T data)
        {
            this.Send(new ThorIOClient.Message(topic, JsonHelper.JsonSerializer<T>(data), this.alias));
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