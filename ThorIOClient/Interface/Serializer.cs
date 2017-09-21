namespace ThorIOClient.Interface
{
    public interface ISerializer
    {
        string Serialize<T>(T t);
        T Deserialize<T>(string jsonString);
    }

}