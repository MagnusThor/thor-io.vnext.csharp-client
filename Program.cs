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
using ThorIOClient.Models;
using ThorIOClient.Serialization;

namespace thorio.csharp
{

    public class vec3{
        public double x;
        public double y;
        public double z;
        public vec3(){

        }
    }

  
    
    [ProxyProperties("mycontroller")]
    public class MyProxy :ProxyBase{
        public MyProxy(){
            this.alias = "mycontroller";  // just temp;
        }
 
        public void sendMessage(double x,double y, double z){
            this.Invoke("invokeAndReturn",new {
                x= x, y=y,z=z
            });
        }

         [Invokable("invokeAndReturn")]
        public void invokeAndReturn(vec3 data){
            Console.WriteLine((data.x+data.y+data.z).ToString());
    }
        
    
      

    }

  
    class Program
    {
        static void Main(string[] args)
        {
            var myproxy = new MyProxy();

              var proxies = new List<ProxyBase>();

            proxies.Add(myproxy);   



            var factory = new Factory("ws://localhost:1337",proxies,null,true);
               // factory.AddProxy(myproxy);


              Random random = new Random();

            myproxy.OnError = (ErrorMessage err) =>
            {
                    Console.WriteLine(err.Message);
            };


            myproxy.OnOpen = (ConnectionInformation ci) =>
            {
                Console.WriteLine("Connected to the controller (proxy)");

                 var timer = new System.Timers.Timer(60);
                timer.Elapsed += (sender, e) =>
                {
                    myproxy.sendMessage(random.NextDouble(),random.NextDouble(),random.NextDouble());
                };

                timer.Start();
            };

            factory.OnOpen = async (ISocket wrapper) =>
            {
                Console.WriteLine("Connected to the server...");
                await myproxy.Connect();
                            
            };


    
           Console.WriteLine("press a key to end...");


            Console.ReadLine();


        }


    }


}
