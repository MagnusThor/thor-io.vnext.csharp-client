using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ThorIOClient;
using ThorIOClient.Models;
using ThorIOClient.Serialization;

namespace thorio.csharp
{

    public class ExampleMessageModel
    {
        public float num;
        public string text;
        public ExampleMessageModel() { }

    }
    [DataContract]

    public class ChatMessageModel
    {
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public string sender { get; set; }

        public string ts { get; set; }

        public ChatMessageModel(string message)
        {
            this.message = message;
        }
    }


    public class TemperatureMessageModel
    {
        public float temp;
        public TemperatureMessageModel() { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            Console.WriteLine("Thor-io.vnext .NET Client.");

            var controllers = new List<string>();
            controllers.Add("rdtest");
            controllers.Add("chat");
            //wss://neordpoc.herokuapp.com
            var factory = new ThorIOClient.Factory("wss://neordpoc.herokuapp.com",
            controllers, new NewtonJsonSerialization());
            //   new JsonSerialization()
            factory.OnOpen = (List<ThorIOClient.Proxy> proxies, ThorIOClient.WebSocketWrapper evt) =>
            {

                Console.WriteLine("Connected to server..");

                var testController = factory.GetProxy("rdtest");
                var chatController = factory.GetProxy("chat");

                chatController.OnError = (string message) =>
                 {
                     Console.WriteLine(message);
                 };


                chatController.OnOpen = (ConnectionInfo message) =>
                {
                    Console.WriteLine("Controller 'chat' is now open");


                    chatController.Invoke("changeGroup", "lobby");

                    chatController.Invoke("changeNickName", "Donald Trump");

                    chatController.On<ChatMessageModel>("chatMessage",
                    (ChatMessageModel chatMessage) =>
                    {
                        Console.WriteLine(chatMessage.message);
                    });

                    var timer = new System.Timers.Timer(2000);

                    timer.Elapsed += (sender, e) =>
                    {

                        var msg = new ChatMessageModel("Sending a message from the C# client");

                        chatController.Invoke("sendChatMessage", msg);

                    };

                    timer.Start();

                };


                testController.OnOpen = (ConnectionInfo message) =>
                {
                    Console.WriteLine("Controller'rdtest'  is open");


                    testController.SetProperty<float>("size", 11);


                    testController.Subscribe<TemperatureMessageModel>("tempChange",
                        (TemperatureMessageModel data) =>
                        {
                            Console.WriteLine("Current temperature {0}", data.temp);
                        }
                    );

                };

                testController.OnClose = (ConnectionInfo message) =>
                {
                    Console.WriteLine("Controller is closed");
                };


                testController.OnError = (string message) =>
                {
                    Console.WriteLine(message);
                };

                testController.On<ExampleMessageModel>("invokeAndSendToAll", (ExampleMessageModel data) =>
                {
                    Console.WriteLine("invokeAndSendToAll - {0}", data.text); // data.num
                });

                testController.On<ExampleMessageModel>("invokeToExpr", (ExampleMessageModel data) =>
               {
                   Console.WriteLine("invokeToExpr - {0}", data.text); // data.num
               });


                testController.Connect();
                chatController.Connect();




            };

            factory.OnClose = (ThorIOClient.WebSocketWrapper evt) =>
            {
                Console.WriteLine("Close");
            };





            Console.ReadLine();


        }
    }


}
