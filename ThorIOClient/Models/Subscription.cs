// using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace ThorIOClient.Models
{
    // [DataContract]
    public class Subscription{
      //  [DataMember(Name = "topic", IsRequired = true)]
        [JsonProperty("topic")]
        public string Topic { get; set; }
        // [DataMember(Name = "controller", IsRequired = true)]
        [JsonProperty("controller")]
        public string Controller { get; set; }

        public Subscription(string topic, string controller){
            this.Topic = topic;
            this.Controller = controller;
        }


    }
}