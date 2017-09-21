
namespace ThorIOClient.Interface
{
    public interface IMessage
    {
        string Data { get; }
        string Controller { get; set; }
        string Topic { get; set; }

    }
}