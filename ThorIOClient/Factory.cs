using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ThorIOClient
{


    public class SubscriptionModel{
        public string Topic;
        public string Controller;

        public SubscriptionModel(string topic, string controller){
            this.Topic = topic;
            this.Controller = topic;
        }


    }

    public partial class Factory
    {
        private List<Proxy> _proxies;
        private WebSocketWrapper ws;
        public Action<List<Proxy>,WebSocketWrapper> OnOpen;
        public Action<WebSocketWrapper> OnClose;
        public Factory(string url, List<string> proxies)
        {
            this._proxies = new List<Proxy>();
            this.ws = WebSocketWrapper.Create(url);
            this.ws.OnConnect((WebSocketWrapper evt) =>
            {
                proxies.ForEach((string proxy) =>
                {
                    this._proxies.Add(new ThorIOClient.Proxy(this.ws, proxy));
                });
                this.OnOpen(this._proxies,evt);

                this.ws.OnMessage( (string data,WebSocketWrapper w) =>
                {
                         var message = JsonHelper.JsonDeserialize<ThorIOClient.Message>(data);
                         var proxy = this.GetProxy(message.Controller);
                         proxy.Dispatch(message);
                        
                });

            });
            this.ws.OnDisconnect((WebSocketWrapper evt) =>
            {
                this.OnClose(evt);
            });


            this.ws.Connect();
        }

        public Proxy GetProxy(string alias){
                return this._proxies.Find( (Proxy pre) => {
                        return pre.alias == alias;
                });
        }
        public WebSocketWrapper Close()
        {
            this.ws.Close();
            return this.ws;
        }


    }
}