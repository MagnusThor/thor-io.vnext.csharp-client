using Newtonsoft.Json;
using System;
using ThorIOClient.Interface;
using System.Collections.Generic;
using System.Linq;

namespace ThorIOClient.Serialization
{
    public class NewtonJsonSerialization : ISerializer
    {
        public T Deserialize<T>(string jsonString)
        {
            try
            {

                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine("HERE:" + jsonString);
            }

            return default(T);
        }

        public string Serialize<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }
        public T DeserializeFromString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        public object DeserializeFromString(string json, System.Type type)
        {
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings());
        }

        

        public IDictionary<string,string> DeserializeFromString(string json, params string[] keys)
        {
            var obj = JsonConvert.DeserializeObject<List<Newtonsoft.Json.Linq.JObject>>(json);
            return new Dictionary<string,string>();
           // return keys.ToDictionary(key => key, key => obj.First().Child(key));
        }


    }
}