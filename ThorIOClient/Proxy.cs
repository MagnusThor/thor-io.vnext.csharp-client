using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ThorIOClient.Attributes;
using ThorIOClient.Interface;
using ThorIOClient.Models;
using ThorIOClient.Serialization;

namespace ThorIOClient {

    public interface IProxyBase {

        WebSocketWrapper Ws {
            get;
            set;
        }
    }

    public interface IPluginCustomEventInfo {

    }
    public class PluginCustomEventInfo: IPluginCustomEventInfo {
        public PluginCustomEventInfo(MethodInfo methodInfo,string alias) {
            MethodInfo = methodInfo;
            ParameterInfo = methodInfo.GetParameters();

            this.Alias = alias;

           

        }

        public string Alias {get;set;}

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

        public void CreateDelegates() {
            this.CustomEvents = new List < PluginCustomEventInfo > ();
            this.Delegates = new Dictionary<string, dynamic>();
            var t = this.GetType();
            var prop = t.GetCustomAttributes(typeof (ProxyProperties)).FirstOrDefault() as ProxyProperties;
            this.alias  = prop.Alias;
            foreach(var member in t.GetMembers()) {
                if (member.GetCustomAttributes(typeof (Invokable), true).Length > 0) {
                    var method = t.GetMethod(member.Name);
                    var methodAlias = member.Name;
                      var invokable = method.GetCustomAttributes(typeof(Invokable),true).First() as Invokable;

                        if(invokable != null) methodAlias = invokable.Alias;
                                     

              
                    this.CustomEvents.Add(new PluginCustomEventInfo(method,methodAlias));
                    
                   

                   
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
                        case 2:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1]), parameters[0],parameters[1]);

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;
                        case 3:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1],parameters[2]), parameters[0],parameters[1],parameters[2]);

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;  
                       case 4:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1],parameters[2],parameters[3]),
                                         parameters[0],parameters[1],parameters[2],parameters[3]);

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;
                       case 5:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1],parameters[2],parameters[3],parameters[4]),
                                         parameters[0],parameters[1],parameters[2],parameters[3],parameters[4]);

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;  
                      case 6:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1],parameters[2],parameters[3],parameters[4],
                                        parameters[5]),
                                         parameters[0],parameters[1],parameters[2],parameters[3],parameters[4],
                                         parameters[5]
                                         );

                                var f = le.Compile();
                                Delegates.Add(method.Name, f);
                            }
                            break;  
                         case 7:
                            {
                                var le = Expression.Lambda(
                                    Expression.Call(
                                        Expression.Constant(this),
                                        method, parameters[0],parameters[1],parameters[2],parameters[3],parameters[4],
                                        parameters[5],parameters[6]),
                                         parameters[0],parameters[1],parameters[2],parameters[3],parameters[4],
                                         parameters[5],parameters[6]
                                         );

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
            var hasDelegate = this.CustomEvents.Count( p => p.Alias == msg.Topic);
            if (hasDelegate > 0) {
                var method = this.CustomEvents.SingleOrDefault(p => p.Alias == msg.Topic);
                ThorIOClient.Extensions.ProxyExtensions.InvokePluginMethod(this,method,msg.Data);

            } else {
                var listener = this.FindListener(msg.Topic);
                if (listener != null) {
                    if (listener.fn != null)
                        listener.fn(msg);
                } else
                    return;
            }
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