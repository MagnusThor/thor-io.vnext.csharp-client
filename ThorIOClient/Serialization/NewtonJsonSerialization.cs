using Newtonsoft.Json;
using System;
using ThorIOClient.Interface;

namespace ThorIOClient.Models
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
    }
}