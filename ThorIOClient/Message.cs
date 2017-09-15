using System;
using System.Runtime.Serialization;

namespace ThorIOClient
{

  public interface IMessage
    {
        string Data { get; }
        string Controller { get; set; }
        string Topic { get; set; }
    
    }

    [DataContract]
    public class Message :IMessage
    {

        [DataMember(Name = "D", IsRequired = false)]
        public string Data { get; private set; }
        [DataMember(Name = "C", IsRequired = true)]
        public string Controller { get; set; }
        [DataMember(Name = "T", IsRequired = true)]
        public string Topic { get; set; }

        public Message(string topic, string data, string controller)
        {
            this.Topic = topic;
            this.Controller = controller;
            this.Data = data;
        }

    }


}
