using System.Runtime.Serialization;

namespace ThorIOClient
{
    [DataContract]
    public class Subscription{
        [DataMember(Name = "topic", IsRequired = true)]
        public string Topic { get; set; }
      [DataMember(Name = "controller", IsRequired = true)]
        public string Controller { get; set; }

        public Subscription(string topic, string controller){
            this.Topic = topic;
            this.Controller = controller;
        }


    }
}