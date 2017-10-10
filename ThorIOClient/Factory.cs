using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThorIOClient.Interface;

namespace ThorIOClient
{

    public partial class Factory
    {
        private List<ProxyBase> _proxies;
        private WebSocketWrapper ws;
        public Action<WebSocketWrapper> OnOpen;
        public Action<WebSocketWrapper> OnClose;


        public ISerializer Serializer { get; set; }


       
        private void AddWsListeners() {
            this.ws.OnConnect((WebSocketWrapper evt) => {
                this.OnOpen(evt);
                this.ws.OnMessage((string data, WebSocketWrapper w) => {
                    var message = Serializer.Deserialize < Models.Message > (data);

              
              
                    var proxy = this.GetProxy < ProxyBase > (message.Controller);

                    if (proxy != null)
                        proxy.Dispatch(message);
                    
                    
                });
            });
            this.ws.OnDisconnect((WebSocketWrapper evt) => {
                this.OnClose(evt);
            });
        }
        //
        public Factory(string url, List<ProxyBase> proxies)
        {
       
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();
            this.ws = WebSocketWrapper.Create(url);
            proxies.ForEach((ProxyBase proxy) =>
            {
                proxy.CreateDelagates();
                proxy.Ws = this.ws;
                this._proxies.Add(proxy);

            });

            this.AddWsListeners();
            this.ws.Connect();


        }


        public Factory(string url, List<string> proxies, ISerializer serializer)
        {
            this.Serializer = serializer;
            this._proxies = new List<ProxyBase>();
            this.ws = WebSocketWrapper.Create(url);

            proxies.ForEach((string proxy) =>
            {
                // Create a generic Proxy?  
                //this._proxies.Add(new ThorIOClient.ProxyBase());
            });

            this.AddWsListeners();
            this.ws.Connect();

        }
        public ProxyBase GetProxy<T>(string alias)
        {

            return (this._proxies.Find((ProxyBase pre) =>
            {
                return pre.alias == alias;
            }));

        }
        public WebSocketWrapper Close()
        {
            this.ws.Close();
            return this.ws;
        }


    }
}