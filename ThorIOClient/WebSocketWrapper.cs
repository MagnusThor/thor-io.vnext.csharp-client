using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThorIOClient.Extensions;
using ThorIOClient.Queue;

namespace ThorIOClient
{


    public interface IWebSocketWrapper{
        
    }

    public class WebSocketWrapper
    {

        public ThreadSafeQueue<byte[]> Queue {get;set;}

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<WebSocketWrapper> _onConnected;
        private Action<string, WebSocketWrapper> _onMessage;
        private Action<WebSocketWrapper> _onDisconnected;

        protected WebSocketWrapper(string uri)
        {
            Queue = new ThreadSafeQueue<byte[]>();

            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;
            
            var queueCancellationToken = new CancellationTokenSource();
                     new Task(action: async () =>
                     {
                         if(this._ws.State ==  WebSocketState.Open){
                            for (var i = 0; i < this.Queue.Count; i++){
                                var bytes = this.Queue.Dequeue();
                                await this.SendMessageAsync(bytes);
                            }
                        }
                     }).Repeat(queueCancellationToken.Token, TimeSpan.FromSeconds(3));
        }


        public static WebSocketWrapper Create(string uri)
        {
            return new WebSocketWrapper(uri);
        }

        public async Task<WebSocketWrapper> Connect()
        {
            await ConnectAsync();
            return this;
        }

        public async Task<WebSocketWrapper> Close()
        {
            await this._ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            return this;
        }
        public WebSocketWrapper OnConnect(Action<WebSocketWrapper> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        public WebSocketWrapper OnDisconnect(Action<WebSocketWrapper> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        public WebSocketWrapper OnMessage(Action<string, WebSocketWrapper> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        public async Task SendMessage(string message)
        {
            await SendMessageAsync(Encoding.UTF8.GetBytes(message));
        }

         public async Task SendMessage(byte[] message)
        {
            await SendMessageAsync(message);
        }

        private async Task SendMessageAsync(byte[] messageBuffer)
        {
            if (_ws.State != WebSocketState.Open){
                this.Queue.Enqueue(messageBuffer);              
            }else{
         
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }
                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
            }
        }
        }

        private async Task ConnectAsync()
        {
            await _ws.ConnectAsync(_uri, _cancellationToken);
            await CallOnConnected();
            await StartListen();
        }

        private async Task StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await
                                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            await CallOnDisconnected();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    await CallOnMessage(stringResult);

                }
            }
            catch (Exception ex)
            {
                await CallOnDisconnected();
            }
            finally
            {
                _ws.Dispose();
            }
        }

        private async Task CallOnMessage(StringBuilder stringResult)
        {
            System.Diagnostics.Debug.WriteLine(stringResult.ToString());

            if (_onMessage != null)
                await RunInTask(() => _onMessage(stringResult.ToString(), this));
        }

        private async Task CallOnDisconnected()
        {
            if (_onDisconnected != null)
                await RunInTask(() => _onDisconnected(this));
        }

        private async Task CallOnConnected()
        {
            if (_onConnected != null)
                await RunInTask(() => _onConnected(this));
        }

        private async static Task RunInTask(Action action)
        {
            await Task.Run(action);//Task.Factory.StartNew(action);
        }
    }
}