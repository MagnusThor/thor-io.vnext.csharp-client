using System.Collections.Generic;

namespace ThorIOClient.Interface
{
    public interface ISerializer
    {
        string Serialize<T>(T t);
        T Deserialize<T>(string jsonString);
        object Deserialize(string json, System.Type type);
        IDictionary<string,string> Deserialize(string json, params string[] keys);
    }
    

}