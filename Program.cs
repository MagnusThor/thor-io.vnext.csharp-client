using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ThorIOClient;
using ThorIOClient.Extensions;
using ThorIOClient.Models;

namespace thorio.csharp
{

    public class ChatMessage{
            
            public long ts {get;set;}
            public string message {get;set;}

            public string sender {get;set;}
            public ChatMessage(string message, string sender){
                this.message = message; 
                this.sender = sender;
                this.ts = DateTime.Now.Ticks;
            }
    }



    
    [ProxyProperties("chat")]
    public class MyProxy :ProxyBase{
        public MyProxy(){
            this.alias = "chat";  // just temp;
        }
 
        public async void SendChatMessageChat(ChatMessage message){
                await this.Invoke("sendChatMessage", message);
              
        }
          [Invokable("GotChatMessage")]
         public void GotChatMessage(ChatMessage msg){
                Console.WriteLine(msg.message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            var proxies = new List<ProxyBase>();
            var myproxy = new MyProxy();

            myproxy.CreateDelagates();

            var json = myproxy.Serializer.Serialize(new ChatMessage("foo","bar"));

            var msg = new ThorIOClient.Models.Message("GotChatMessage",json,"chat");

       

            myproxy.Dispatch(msg);

            // proxies.Add(myproxy);




            //  var factory = new ThorIOClient.Factory("ws://neordpoc.herokuapp.com",
            //      proxies);

            // myproxy.OnError = (string err) => {

            // };  


            // myproxy.OnOpen = (ConnectionInformation ci) => {
            //         Console.WriteLine("Connected to controller - chat");

            //              var timer = new System.Timers.Timer(4000);
            //             timer.Elapsed += (sender, e) =>
            //             {

            //                 myproxy.SendChatMessageChat(new ChatMessage("Hello World from ProxyBase","God"));

            //             };

            //             timer.Start();

            // };  

            // factory.OnOpen = (WebSocketWrapper wrapper) => {

            //         Console.WriteLine("Connected to the server...");


            //         myproxy.Connect();
            // };

             


            

            
               

            // var graph = new Dictionary<string,ParameterInfo[]>();

            // Type t = typeof(Animal);

            // var instance = Activator.CreateInstance(typeof(Animal));   

            // Console.WriteLine("Members of {0}:", t.Name);

            //     // ProxyName = t.Name

            //   foreach (var member in t.GetMembers()){
            //         if(member.GetCustomAttributes(typeof(Invokable),true).Length > 0){
            //         var methodInfo = t.GetMethod(member.Name);
            //         var parameterInfo = methodInfo.GetParameters();
            //         graph.Add(member.Name,parameterInfo); //
            //         }       
            //   }

            // // How to invoke

            // var message = new ThorIOClient.Models.Message("ChangePerson","{'name':'John Doe','age':42}","Animal");

            // var json = new NewtonJsonSerialization();

            // var data = json.Deserialize<object>(message.Data) ;
            // Console.WriteLine(data);

            // // instance.GetType().InvokeMember(message.Topic,BindingFlags.InvokeMethod,null,instance,
            // new object[]{"data",42}
            // );


        //    var assembly = System.Reflection.Assembly.GetAssembly(typeof(Animal));

        //     var m = assembly.GetCustomAttributes(typeof(Invokable),true);

        //     foreach(var p in m){
        //         Console.WriteLine(p);
        //     }
        //     //     Console.Clear();

            //     queue = new ThorIOClient.Queue.ThreadSafeQueue<NetworkEventMessage<Vec3>>();



            //     Console.WriteLine("Thor-io.vnext .NET Client.");

            //     var controllers = new List<string>();
            //     controllers.Add("rocketGame");
            //     //wss://neordpoc.herokuapp.com
            //     var factory = new ThorIOClient.Factory("ws://neordpoc.herokuapp.com",
            //     controllers, new NewtonJsonSerialization());
            //     //   new JsonSerialization()
            //     factory.OnOpen = async (List<ThorIOClient.Proxy> proxies, ThorIOClient.WebSocketWrapper evt) =>
            //     {


            //         var rocketGame = factory.GetProxy("rocketGame");
            //         Console.WriteLine("Connected to server..");

            //         rocketGame.OnError = (string err) => {
            //             // do op 
            //         };

            //         rocketGame.OnOpen = async (ConnectionInformation ci) =>
            //         {

            //             Console.WriteLine("connected to RocketGame");
            //             Random random = new Random();


            //             var timer = new System.Timers.Timer(1000);
            //             timer.Elapsed += (sender, e) =>
            //             {

            //                     int rnd = random.Next(0, 60);
            //                     for(var i=0;i<rnd;i++){

            //                     var vec3 = new Vec3( 
            //                         (float.MaxValue * ((random.Next() / 1073741824.0f) - 1.0f)),
            //                          (float.MaxValue * ((random.Next() / 1073741824.0f) - 1.0f)),
            //                           (float.MaxValue * ((random.Next() / 1073741824.0f) - 1.0f)));

            //                     queue.Enqueue(new NetworkEventMessage<Vec3>(vec3));

            //                 }
            //             };

            //             timer.Start();


            //      var ct = new CancellationTokenSource();

            //          new Task(async () => 
            //          {

            //              for(var i = 0; i < queue.Count;i++){
            //                     var msg = queue.Dequeue();
            //                     await rocketGame.Invoke< NetworkEventMessage<Vec3>>("moveRocket",
            //                     msg);
            //              }

            //          }).Repeat(ct.Token, TimeSpan.FromMilliseconds(60));



            //         };

            //         await rocketGame.Connect();
            //     };

            //     factory.OnClose = (ThorIOClient.WebSocketWrapper evt) =>
            //     {
            //         Console.WriteLine("Close");
            //     };



            // var data = new List<byte>();
            // data.Add(0);
            // data.Add(1);
            // data.Add(3);
            // data.Add(4);
            // data.Add(5);

            // // add some bytes

            // // Create a new dataFrame 
            // var dataFrame = new ThorIOClient.Protocol.DataFrame(data);
        
            // foreach (var b in data)
            // {
            //     Console.Write(b.ToString()+ ",");
            // }
            // // Display dataframe content, this is the content to send as well
            // Console.WriteLine();
            // foreach (var b in dataFrame.ToBytes())
            // {
            //     Console.Write(b.ToString() + ",");
            // }


            // // Read the frame

            // Console.WriteLine();

            // var arr = new List<Byte>(dataFrame.ToBytes());

            // ThorIOClient.Protocol.DataFrameReader.Read(arr
            //     , new ThorIOClient.Protocol.ReadState(),
            // (ThorIOClient.Protocol.FrameType type, byte[] result) =>
            // {

            //     foreach (var b in result)
            //     {
            //         Console.Write(b.ToString()+ ",");
            //     }

            // });







            Console.ReadLine();


        }


    }


}
