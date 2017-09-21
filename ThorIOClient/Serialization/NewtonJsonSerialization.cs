using Newtonsoft.Json;
using ThorIOClient.Interface;

namespace ThorIOClient.Serialization
 {
    public class NewtonJsonSerialization : ISerializer
    {
        public T Deserialize<T>(string jsonString)
        { 
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public string Serialize<T>(T t)
        {
           return JsonConvert.SerializeObject(t);
        }
    }
}