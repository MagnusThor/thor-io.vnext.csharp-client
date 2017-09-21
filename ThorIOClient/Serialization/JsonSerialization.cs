// using System.IO;
// using System.Runtime.Serialization.Json;
// using System.Text;
// using ThorIOClient.Interface;

// namespace ThorIOClient.Serialization
// {
//     public class JsonSerialization: ISerializer
//     {
//         public string Serialize<T>(T t)
//         {
//             DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
//             MemoryStream ms = new MemoryStream();
//             ser.WriteObject(ms, t);
//             var jsonString = Encoding.UTF8.GetString(ms.ToArray());
//             ms.Close();
//             return jsonString;
//         }
//         public T Deserialize<T>(string jsonString)
//         {
//             DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
//             MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
//             T obj = (T)ser.ReadObject(ms);
//             return obj;
//         }
//     }

// }
