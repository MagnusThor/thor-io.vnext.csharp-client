using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ThorIOClient;
using ThorIOClient.Attributes;
using ThorIOClient.Extensions;
using ThorIOClient.Interfaces;
using ThorIOClient.Serialization;

namespace thorio.csharp
{

    public class ChatMessage{
            
            public string ts {get;set;}
            public string message {get;set;}

            public string sender {get;set;}
            public ChatMessage(string message, string sender){
                this.message = message; 
                this.sender = sender;
                this.ts = DateTime.Now.Ticks.ToString();
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
        [Invokable("chatMessage")]
         public void GotChatMessage(ChatMessage msg){
                Console.WriteLine(msg.message);
        }
         [Invokable("SingleParam")]
         public void SingleParam(int age,string name){
                Console.WriteLine(age);
                Console.WriteLine(name);
                

        }
       

    }

    public class TestModel
    {
        public string Test1 { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var _proxy = new MyProxy();
            //_proxy.alias = "test";
            //_proxy.On<TestModel>("test", (x) => {
            //    Console.WriteLine(x.Test1);
            //});

            //var _data = new NewtonJsonSerialization().Serialize<TestModel>(new TestModel() { Test1 = "OK" });
            //var _payload = new Message("test", _data , "test");

            //_proxy.Dispatch(_payload);

            //var factory = new Factory("ws://neordpoc.herokuapp.com", new List<string> { })

            //var proxies = new List<ProxyBase>();
            var myproxy = new MyProxy();


            //proxies.Add(myproxy);

            // myproxy.CreateDelegates();

            //var json = myproxy.Serializer.Serialize(new ChatMessage("foo","bar")); // GotChatMessage
            //var json = myproxy.Serializer.Serialize<object>(new { age = 21, name = "foo" }); // SingleParam

            //var d = myproxy.Serializer.Deserialize("foo", typeof(string));

            //var msg = new Message("SingleParam", json, "chat");


            //myproxy.Dispatch(msg);



            //proxies.Add(myproxy);




            var factory = new Factory("ws://neordpoc.herokuapp.com");

            factory.AddProxy(myproxy);

            myproxy.OnError = (string err) =>
            {

            };


            myproxy.OnOpen = (ConnectionInformation ci) =>
            {
                Console.WriteLine("Connected to controller - chat");

                var timer = new System.Timers.Timer(4000);
                timer.Elapsed += (sender, e) =>
                {

                    myproxy.SendChatMessageChat(new ChatMessage("Hello World from ProxyBase", "Church boy"));

                };

                timer.Start();

            };

            factory.OnOpen = async (ISocket wrapper) =>
            {

                Console.WriteLine("Connected to the server...");
                await myproxy.Connect();
                myproxy.SendChatMessageChat(new ChatMessage("Hello World from ProxyBase (ISocket)", "Church boy"));
            };




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
