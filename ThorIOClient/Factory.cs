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


using ThorIOClient.Interfaces;
using ThorIOClient.Models;
using ThorIOClient.Attributes;

namespace ThorIOClient
{

    public partial class Factory
    {
        private List<ProxyBase> _proxies;
        private ISocket socket;
        public Action<ISocket> OnOpen;
        public Action<ISocket> OnClose;
        public ISerializer Serializer { get; set; }

        static Factory getInstance(string url){
                return new Factory(url);
        }

        private void AddWsListeners() {
            this.socket.OnConnect((Action<ISocket>)((ISocket evt) => {
                this.OnOpen(evt);
                this.socket.OnMessage((Action<string, ISocket>)((string data, ISocket w) => {
                    var message = Serializer.Deserialize <Message > ((string)data);
                    var proxy = this.GetProxy <ProxyBase> (message.Controller);

                    if (proxy != null)
                        proxy.Dispatch(message);
                }));
            }));
            this.socket.OnDisconnect((ISocket evt) => {
                this.OnClose(evt);
            });
        }

        public Factory(string url, ISocket socket = null, bool autoConnect = true){
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.socket = socket;

            this.AddWsListeners();

            if (autoConnect)
                Task.Run(() => { this.socket.Connect(); }).ConfigureAwait(false);
        }
        public Factory(string url, List<ProxyBase> proxies, ISocket socket = null, bool autoConnect = true)
        {
            this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.socket = socket;

            proxies.ForEach((ProxyBase proxy) =>
            {
                proxy.CreateDelegates();
                proxy.Socket = this.socket;
                this._proxies.Add(proxy);
            });

            this.AddWsListeners();

            if (autoConnect)
                Task.Run(() => { this.socket.Connect(); }).ConfigureAwait(false);
        }
        public Factory(string url, List<string> proxies, ISocket socket = null, bool autoConnect = true)
        {
           this.Serializer = new ThorIOClient.Serialization.NewtonJsonSerialization();
            this._proxies = new List<ProxyBase>();

            if (socket == null)
                socket = new WebSocketWrapper(url);

            this.socket = socket;

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
                Task.Run(() => { this.socket.Connect(); }).ConfigureAwait(false);
        }

        public void AddProxy(ProxyBase proxy)
        {
            if (this._proxies == null)
                this._proxies = new List<ProxyBase>();

            if (!_proxies.Any(x => x.alias.ToLower() == proxy.alias.ToLower()))
            { 
                proxy.CreateDelegates();
                proxy.Socket = this.socket;
                this._proxies.Add(proxy);
            }
        }
        public async Task RemoveProxy(ProxyBase proxy)
        {
            await proxy.Close().ConfigureAwait(false);
            this._proxies.Remove(proxy);
        }

        // Nothing to see here, move along... Experimental
        // public IEnumerable<ProxyBase> AutomapProxies()
        // {
        //     var proxies = GetDeclaredProxyBases();
        //     foreach(var proxy in proxies)
        //     {
        //         AddProxy(proxy);
        //     }
        //     return proxies;
        // }

        // public IEnumerable<ProxyBase> GetDeclaredProxyBases()
        // {
        //     var instances = AppDomain.CurrentDomain.GetAssemblies()
        //                     .SelectMany(s => s.GetTypes())
        //                     .Where(x => typeof(ProxyBase).IsAssignableFrom(x) && x.GetCustomAttributes(typeof(ProxyProperties)).Any() && x.GetConstructor(Type.EmptyTypes) != null)
        //                     .Select(x => Activator.CreateInstance(x) as ProxyBase);

        //     return instances;
        // }

        public ProxyBase GetProxy<T>(string alias)
        {
            return (this._proxies.Find((ProxyBase pre) =>
            {
                return pre.alias == alias;
            }));

        }
        public async Task<ISocket> Close()
        {
            await this.socket.Close().ConfigureAwait(false);
            return this.socket;
        }
    }
}