
namespace ThorIOClient.Interfaces
{
    public interface IMessage
    {
        string Data { get; }
        string Controller { get; set; }
        string Topic { get; set; }
    }
}