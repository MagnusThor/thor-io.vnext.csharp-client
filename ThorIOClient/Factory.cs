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
using thorio.csharp.ThorIOClient;
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

        static Factory getInstance(string url){
                return new Factory(url);
        }

        private void AddWsListeners() {
            this.ws.OnConnect((Action<WebSocketWrapper>)((WebSocketWrapper evt) => {
                this.OnOpen(evt);
                this.ws.OnMessage((Action<string, WebSocketWrapper>)((string data, WebSocketWrapper w) => {
                    var message = Serializer.Deserialize <Models.Message > ((string)data);
                    var proxy = this.GetProxy <ProxyBase> (message.Controller);

                    if (proxy != null)
                        proxy.Dispatch(message);
                }));
            }));
            this.ws.OnDisconnect((WebSocketWrapper evt) => {
                this.OnClose(evt);
            });
        }

        public Factory(string url, bool autoConnect = true){
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();
            this.ws = WebSocketWrapper.Create(url);
            this.AddWsListeners();

            if (autoConnect)
                Task.Run(() => { this.ws.Connect(); });
        }

        public Factory(string url, List<ProxyBase> proxies, bool autoConnect = true)
        {
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();
            this.ws = WebSocketWrapper.Create(url);
            proxies.ForEach((ProxyBase proxy) =>
            {
                proxy.CreateDelegates();
                proxy.Ws = this.ws;
                this._proxies.Add(proxy);
            });

            this.AddWsListeners();

            if (autoConnect)
                Task.Run(() => { this.ws.Connect(); });
        }

        public void AddProxy(ProxyBase proxy)
        {
            if (this._proxies == null)
                this._proxies = new List<ProxyBase>();

            if (!_proxies.Any(x => x.alias.ToLower() == proxy.alias.ToLower()))
            { 
                proxy.CreateDelegates();
                proxy.Ws = this.ws;
                this._proxies.Add(proxy);
            }

            //Throw?
        }

        public void RemoveProxy(ProxyBase proxy)
        {
            throw new NotImplementedException();
        }

        public Factory(string url, List<string> proxies)
        {
           this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();
            this.ws = WebSocketWrapper.Create(url);

            if (proxies.Count == 0)
                this._proxies.Add(new GenericProxy("generic"));
            else
            { 
                proxies.ForEach((string proxy) =>
                {
                    // Create a generic Proxy?  
                    this._proxies.Add(new GenericProxy(proxy));
                });
            }
            this.AddWsListeners();
            Task.Run(() => { this.ws.Connect(); });
        }
        public ProxyBase GetProxy<T>(string alias)
        {

            return (this._proxies.Find((ProxyBase pre) =>
            {
                return pre.alias == alias;
            }));

        }
        public async Task<WebSocketWrapper> Close()
        {
            await this.ws.Close();
            return this.ws;
        }


    }
}