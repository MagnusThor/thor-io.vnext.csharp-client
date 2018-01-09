using Newtonsoft.Json;
using System;
using ThorIOClient.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ThorIOClient.Serialization
{
    public class NewtonJsonSerialization : ISerializer
    {
        // public T Deserialize<T>(string jsonString)
        // {
        //     try
        //     {

        //         return JsonConvert.DeserializeObject<T>(jsonString);
        //     }
        //     catch (Exception ex)
        //     {
        //         //System.Diagnostics.Debug.WriteLine("HERE:" + jsonString);
        //     }

        //     return default(T);
        // }

        // public string Serialize<T>(T t)
        // {
        //     return JsonConvert.SerializeObject(t);
        // }

         public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string Serialize(object obj, Type type)
        {
            return JsonConvert.SerializeObject(obj, type, Formatting.None, new JsonSerializerSettings());
        }
        public T Deserialize<T>(string json)
        {
            try{
                  return JsonConvert.DeserializeObject<T>(json);
            }catch(Exception ex){
                // todo: Throw an error message?
            }
              return default(T);
        }
        public object Deserialize(string json, System.Type type)
        {
            if (type == typeof(string) && !this.IsValidJson(json))
                json = this.Serialize(json);

            var value =  JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings());
            return value;
        }

        public bool IsValidJson(string s)
        {
            s = s.Trim();
            if ((s.StartsWith("{") && s.EndsWith("}")) || //For object
                (s.StartsWith("[") && s.EndsWith("]")) || //For array
                (s.StartsWith("\"") && s.EndsWith("\""))) //For JSON string value
            {
                try
                {
                    var obj = JToken.Parse(s);
                    return true;
                }
                catch
                {               
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public IDictionary<string,string> Deserialize(string json, params string[] keys)
        {
            var obj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);    
            return new Dictionary<string, string>(obj.Properties().ToDictionary(pair => pair.Name, pair => pair.Value.ToString()));
        }
    }
}