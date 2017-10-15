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
using thorio.csharp.ThorIOClient.Interfaces;
using ThorIOClient.Interfaces;

namespace ThorIOClient
{

    public partial class Factory
    {
        private List<ProxyBase> _proxies;
        private ISocket ws;
        public Action<ISocket> OnOpen;
        public Action<ISocket> OnClose;
        public ISerializer Serializer { get; set; }

        static Factory getInstance(string url){
                return new Factory(url);
        }

        private void AddWsListeners() {
            this.ws.OnConnect((Action<ISocket>)((ISocket evt) => {
                this.OnOpen(evt);
                this.ws.OnMessage((Action<string, ISocket>)((string data, ISocket w) => {
                    var message = Serializer.Deserialize <Interfaces.Message > ((string)data);
                    var proxy = this.GetProxy <ProxyBase> (message.Controller);

                    if (proxy != null)
                        proxy.Dispatch(message);
                }));
            }));
            this.ws.OnDisconnect((ISocket evt) => {
                this.OnClose(evt);
            });
        }

        public Factory(string url, ISocket socket = null, bool autoConnect = true){
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.ws = socket;

            this.AddWsListeners();

            if (autoConnect)
                Task.Run(() => { this.ws.Connect(); });
        }
        public Factory(string url, List<ProxyBase> proxies, ISocket socket = null, bool autoConnect = true)
        {
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.ws = socket;

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
        public Factory(string url, List<string> proxies, ISocket socket = null, bool autoConnect = true)
        {
           this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.ws = socket;

            if (proxies.Count == 0)
                this._proxies.Add(new GenericProxy("generic"));
            else
            { 
                proxies.ForEach((string proxy) =>
                {
                    this._proxies.Add(new GenericProxy(proxy));
                });
            }
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
        }
        public async Task RemoveProxy(ProxyBase proxy)
        {
            await proxy.Close();
            this._proxies.Remove(proxy);
        }

        //need to discuss IProxybase (and create delegates call issue)
        public IEnumerable<IProxyBase> GetDeclaredProxies()
        {
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(IProxyBase)) && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as IProxyBase;

            return instances;
        }

        public ProxyBase GetProxy<T>(string alias)
        {
            return (this._proxies.Find((ProxyBase pre) =>
            {
                return pre.alias == alias;
            }));

        }
        public async Task<ISocket> Close()
        {
            await this.ws.Close();
            return this.ws;
        }
    }
}