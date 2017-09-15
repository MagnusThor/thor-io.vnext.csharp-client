using System;
using System.Collections.Generic;
using ThorIOClient;

namespace thorio.csharp
{

    public class TempModel
    {
        public float num;
        public string text;
        public TempModel()
        {

        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            Console.Clear();
            
            Console.WriteLine("ThorIO .NET Client..");

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


                testController.OnOpen = (ConnectionInfo message) => {
                             Console.WriteLine("Controller is open");
                };

                testController.OnClose = (ConnectionInfo message) => {
                        Console.WriteLine("Controller is closed");
                };


                 testController.OnError = (string message) => {
                             Console.WriteLine(message);
                };
              
                testController.On<TempModel>("invokeAndSendToAll", (TempModel data) =>
                {
                    Console.WriteLine(data.text); // data.num
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
