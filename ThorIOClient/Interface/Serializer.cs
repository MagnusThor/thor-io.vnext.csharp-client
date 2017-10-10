using System.Collections.Generic;

namespace ThorIOClient.Interface
{
    public interface ISerializer
    {
        string Serialize<T>(T t);
        T Deserialize<T>(string jsonString);

       T DeserializeFromString<T>(string json);

       object DeserializeFromString(string json, System.Type type);

       // object DeserializeFromString(string json, string typeName);

        IDictionary<string,string> DeserializeFromString(string json, params string[] keys);
    }
    

}