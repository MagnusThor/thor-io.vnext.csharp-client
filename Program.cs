using System;
using System.Collections.Generic;
using ThorIOClient;

namespace thorio.csharp
{

    public class ExampleMessageModel
    {
        public float num;
        public string text;
        public ExampleMessageModel(){}
        
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

            var factory = new ThorIOClient.Factory("ws://localhost:1337/", controllers);

            factory.OnOpen = (List<ThorIOClient.Proxy> Proxies, ThorIOClient.WebSocketWrapper evt) =>
            {

                Console.WriteLine("Connected to server..");
                // var testController = Proxies.Find((ThorIOClient.Proxy pre) =>
                // {
                //     return pre.alias == "rdtest";
                // });
                var testController = factory.GetProxy("rdtest");

                testController.OnOpen = (ConnectionInfo message) =>
                {
                    Console.WriteLine("Controller is open");


                    testController.SetProperty<float>("size",11);

                   
                    testController.Subscribe<TemperatureMessageModel>("tempChange",
                        (TemperatureMessageModel data) => {
                                Console.WriteLine("Current temperature {0}",data.temp);
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
                    Console.WriteLine("invokeAndSendToAll - {0}",data.text); // data.num
                });

                 testController.On<ExampleMessageModel>("invokeToExpr", (ExampleMessageModel data) =>
                {
                    Console.WriteLine("invokeToExpr - {0}", data.text); // data.num
                });

                testController.Connect();




            };

            factory.OnClose = (ThorIOClient.WebSocketWrapper evt) =>
            {
                Console.WriteLine("Close");
            };





            Console.ReadLine();


        }
    }


}
