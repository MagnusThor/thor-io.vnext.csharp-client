using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ThorIOClient.Interface;
using ThorIOClient.Models;
using ThorIOClient.Serialization;

namespace ThorIOClient {

    public static class ProxyHelpers {

        // public static void InvokePluginMethod<T>(this T plugin, MethodInfo methodInfo, dynamic[] parameters)
        //     where T : ProxyBase, IProxyBase
        // {
        //     if (methodInfo.ReturnType == typeof(void))
        //     {
        //         plugin.InvokeVoid(methodInfo.Name, parameters);
        //     }else
        //         throw new NotImplementedException();


        // }

    }

    public interface IProxyBase {

        WebSocketWrapper Ws {
            get;
            set;
        }
    }
    // public class ProxyBase
    // {
    //     // internal void InvokeWithVoid<T>(this T plugin, string key, params dynamic[] p) where T : Proxy
    // {
    //       if (plugin.Delegates == null)
    //         plugin.CreateDelegates();
    //     if (p == null)
    //     {            
    //         plugin.Delegates[key]();
    //         return;
    //     }
    //     switch (p.Length)
    //     {
    //         case 0:
    //             plugin.Delegates[key]();
    //             break;
    //         case 1:
    //             plugin.Delegates[key](p[0]);
    //             break;
    //         case 2:
    //             plugin.Delegates[key](p[0], p[1]);
    //             break;
    //         case 3:
    //             plugin.Delegates[key](p[0], p[1], p[2]);
    //             break;
    //         case 4:
    //             plugin.Delegates[key](p[0], p[1], p[2], p[3]);
    //             break;
    //         case 5:
    //             plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4]);
    //             break;
    //         case 6:
    //             plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5]);
    //             break;
    //         case 7:
    //             plugin.Delegates[key](p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
    //             break;                
    //     }
    // }
    // }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Invokable: Attribute {
        public string Alias {
            get;
            set;
        }
        public Invokable(string alias) {
            this.Alias = alias;
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProxyProperties: Attribute {
        public string Alias {
            get;
            private set;
        }
        public ProxyProperties(string alias) {
            this.Alias = alias;
        }

    }

    public interface IPluginCustomEventInfo {

    }
    public class PluginCustomEventInfo: IPluginCustomEventInfo {
        public PluginCustomEventInfo(MethodInfo methodInfo) {
            MethodInfo = methodInfo;
            ParameterInfo = methodInfo.GetParameters();
        }

        public MethodInfo MethodInfo {
            get;
            set;
        }
        public ParameterInfo[] ParameterInfo {
            get;
            private set;
        }
    }


    public partial class ProxyBase :IProxyBase{

        public ISerializer Serializer {
            get;
            set;
        }
        public WebSocketWrapper Ws;
        public string alias;
        private List < Listener > Listeners;

        public Dictionary < string, dynamic > Delegates;

        List < PluginCustomEventInfo > CustomEvents {
            get;
            set;
        }

        public void CreateDelagates() {
          
            this.CustomEvents = new List < PluginCustomEventInfo > ();
            this.Delegates = new Dictionary<string, dynamic>();

            var t = this.GetType();
            var ca = t.GetCustomAttributes(typeof (ProxyProperties)).FirstOrDefault() as ProxyProperties;

            this.alias  = ca.Alias;

            foreach(var member in t.GetMembers()) {
                if (member.GetCustomAttributes(typeof (Invokable), true).Length > 0) {

                    var method = t.GetMethod(member.Name);

                    var pc = new PluginCustomEventInfo(method);
                
                    this.CustomEvents.Add(new PluginCustomEventInfo(methodInfo: method));

                    var key = member.GetCustomAttribute(typeof (Invokable)) as Invokable;
                    var parameters = method.GetParameters()
                        .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name)).ToList();

                    switch (parameters.Count) {
                        case 0:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method));

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;
                        case 1:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0]), parameters[0]);

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;

                    };

                }
            }
        }




        public ProxyBase() {


            this.Serializer = new NewtonJsonSerialization();
            this.Listeners = new List < Listener > ();

            this.On < Models.ConnectionInformation > ("___open", (Models.ConnectionInformation info) => {
                this.OnOpen(info);
            });

            this.On < string > ("___error", (string err) => {
                this.OnError(err);
            });

        }
        public Action < Models.ConnectionInformation > OnOpen;
        public Action < Models.ConnectionInformation > OnClose;
        public Action < string > OnError;

        public bool IsConnected {
            get;
            private set;
        }
        WebSocketWrapper IProxyBase.Ws { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task Connect() {
            await this.Send(new Models.Message("___connect", "", this.alias));
        }
        private async Task Send(Models.Message message) {
            try {
                await Task.Run(() => {
                    var data = Serializer.Serialize < Models.Message > (message);
                    this.Ws.SendMessage(data);
                });
            } catch (Exception ex) {
                this.OnError(ex.Message);
            }

        }
        public async Task < ProxyBase > Close() {
            await this.Send(new Models.Message("___close", "", this.alias));
            return this;
        }
        public ProxyBase On < T > (string topic, Action < T > fn) {

            if (typeof (T) == typeof (IMessage)) {
                this.Listeners.Add(new Listener(topic, message => fn((T) message)));
            } else {
                var listener = new Listener(topic, message => fn(Serializer.Deserialize < T > (message.Data)));
                this.Listeners.Add(listener);
            }


            return this;
        }
        public ProxyBase Off(string name) {
            var listener = this.FindListener(name);
            this.Listeners.Remove(listener);
            return this;

        }
        public async Task < ProxyBase > Subscribe < T > (string topic, Action < T > fn) {

            var message = new Models.Message("___subscribe", Serializer.Serialize < Models.Subscription > (new Models.Subscription(topic, this.alias)), this.alias);

            await this.Send(message);
            this.On < T > (topic, fn);

            return this;

        }

        public async Task < ProxyBase > UnSubscribe(string topic) {
            var message = new Models.Message("___unsubscribe",
                Serializer.Serialize < Models.Subscription > (new Models.Subscription(topic, this.alias)), this.alias);
            await this.Send(message);
            this.Off(topic);
            return this;
        }
        public async Task < ProxyBase > Invoke < T > (string topic, T data) {
            await this.Send(new Models.Message(topic, Serializer.Serialize < T > (data), this.alias));
            return this;
        }
        public async Task < ProxyBase > Publish < T > (string topic, T data) {
            await this.Invoke < T > (topic, data);
            return this;
        }

        public async Task < ProxyBase > SetProperty < T > (string propName, T propValue) {
            await this.Invoke < T > (propName, propValue);
            return this;
        }

        public void Dispatch(IMessage msg) {

       
            var d = this.Delegates.ContainsKey(msg.Topic); 

            if(d){
                var t = this.CustomEvents.SingleOrDefault( p => p.MethodInfo.Name == msg.Topic);
                var dt = this.Serializer.DeserializeFromString(msg.Data,t.ParameterInfo[0].ParameterType);
        
                var param = ThorIOClient.Extensions.ProxyExtensions.ExtractMethodParameters(
                t.ParameterInfo,
                msg.Data);
            


               

                ThorIOClient.Extensions.ProxyExtensions.InvokeWithVoid(this,t.MethodInfo.Name,param);






                //this.Delegates[msg.Topic](dt);
                

            }
         


            var listener = this.FindListener(msg.Topic);
            if (listener != null) {
                //System.Diagnostics.Debug.WriteLine("CALLING LISTENER FUNCTION with data: " + msg.Data);
                if (listener.fn != null)
                    listener.fn(msg);
            } else
                return;


        }
        private Listener FindListener(string topic) {
            return this.Listeners.Find((Listener pre) => {
                return pre.Topic == topic;
            });
        }


    }


}



// private Func<int, string> FindMethodByAccessor(string accessor)
// {
//     return;
//     // var desiredMethod = this.GetType().GetMethods()
//     //           .Where(x => x.GetCustomAttributes(typeof(Invokable), false).Length > 0)
//     //           .Where(y => (y.GetCustomAttributes(typeof(Invokable), false).First() as Invokable).Accessor == accessor)
//     //           .FirstOrDefault();
//     // if (desiredMethod == null) return null;

//     // ParameterExpression px = Expression.Parameter(typeof(int));
//     // ConstantExpression instance = Expression.Constant(this);
//     // return Expression.Lambda<Func<int, string>>(
//     //    Expression.Call(instance, desiredMethod, new Expression[] { px }), px).Compile();

// }