using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThorIOClient.Interface;

namespace ThorIOClient
{
    public partial class Proxy
    {
        private ISerializer Serializer { get; set; }
        private WebSocketWrapper ws;
        public string alias;
        private List<Listener> Listeners;
        public Proxy(WebSocketWrapper ws, string alias, ISerializer serializer)
        {

            this.Serializer = serializer;
            this.Listeners = new List<Listener>();
            this.ws = ws;
            this.alias = alias;

            this.On<Models.ConnectionInformation>("___open", (Models.ConnectionInformation info) =>
            {
                this.OnOpen(info);
            });

            this.On<string>("___error", (string err) =>
            {
                this.OnError(err);
            });

        }
        public Action<Models.ConnectionInformation> OnOpen;
        public Action<Models.ConnectionInformation> OnClose;
        public Action<string> OnError;

        public bool IsConnected { get; private set; }

        public async Task Connect()
        {
            await this.Send(new Models.Message("___connect", "", this.alias));
        }
        private async Task Send(Models.Message message)
        {
            try
            {
                await Task.Run(() => {
                    var data = Serializer.Serialize<Models.Message>(message);
                    this.ws.SendMessage(data);
                });
            }
            catch (Exception ex)
            {
                this.OnError(ex.Message);
            }

        }
        public async Task<Proxy> Close()
        {
            await this.Send(new Models.Message("___close", "", this.alias));
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
        public async Task<Proxy> Subscribe<T>(string topic, Action<T> fn)
        {

            var message = new Models.Message("___subscribe", Serializer.Serialize<Models.Subscription>(new Models.Subscription(topic, this.alias)), this.alias);

            await this.Send(message);
            this.On<T>(topic, fn);

            return this;

        }

        public async Task<Proxy> UnSubscribe(string topic)
        {
            var message = new Models.Message("___unsubscribe",
            Serializer.Serialize<Models.Subscription>(new Models.Subscription(topic, this.alias)), this.alias);
            await this.Send(message);
            this.Off(topic);
            return this;
        }
        public async Task<Proxy> Invoke<T>(string topic, T data)
        {
            await this.Send(new Models.Message(topic, Serializer.Serialize<T>(data), this.alias));
            return this;
        }
        public async Task<Proxy> Publish<T>(string topic, T data)
        {
            await this.Invoke<T>(topic, data);
            return this;
        }

        public async Task<Proxy> SetProperty<T>(string propName, T propValue)
        {
            await this.Invoke<T>(propName, propValue);
            return this;
        }


        public void Dispatch(IMessage msg)
        {

            var listener = this.FindListener(msg.Topic);
            if (listener != null)
            {
                //System.Diagnostics.Debug.WriteLine("CALLING LISTENER FUNCTION with data: " + msg.Data);
                if (listener.fn != null)
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