using Newtonsoft.Json;
using System;
using ThorIOClient.Interface;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

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

         public string SerializeToString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string SerializeToString(object obj, Type type)
        {
            return JsonConvert.SerializeObject(obj, type, Formatting.None, new JsonSerializerSettings());
        }
        public T DeserializeFromString<T>(string json)
        {
            
            return JsonConvert.DeserializeObject<T>(json);
        }
        public object DeserializeFromString(string json, System.Type type)
        {
            if (type == typeof(string) && !this.IsValidJson(json))
                json = this.SerializeToString(json);

            var value =  JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings());
            return value;
        }



        public bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]")) || //For array
                (strInput.StartsWith("\"") && strInput.EndsWith("\""))) //For JSON string value
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    //Exception in parsing json                    
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

   
        public IDictionary<string,string> DeserializeFromString(string json, params string[] keys)
        {
            
            var obj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
    
            var r =  new Dictionary<string, string>(obj.Properties().ToDictionary(pair => pair.Name, pair => pair.Value.ToString()));

             return r;
         
           // return keys.ToDictionary(key => key, key => obj.First().Child(key));
        }


    }
}