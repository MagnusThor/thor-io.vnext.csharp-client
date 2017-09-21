using System;
using Newtonsoft.Json;
// using System.Runtime.Serialization;
using ThorIOClient.Interface;

namespace ThorIOClient.Models
{
 
    // [DataContract]
    public class Message :IMessage
    {

        // [DataMember(Name = "D", IsRequired = false)]
        [JsonProperty("D")]
        public string Data { get; private set; }
       // [DataMember(Name = "C", IsRequired = true)]
        [JsonProperty("C")]
        public string Controller { get; set; }
        // [DataMember(Name = "T", IsRequired = true)]
        [JsonProperty("T")]
        public string Topic { get; set; }
        public Message(string topic, string data, string controller)
        {
            this.Topic = topic;
            this.Controller = controller;
            this.Data = data;
        }
    }
}
