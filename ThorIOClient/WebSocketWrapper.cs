using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThorIOClient.Extensions;
using ThorIOClient.Interfaces;
using ThorIOClient.Queue;

namespace ThorIOClient
{
    public class WebSocketWrapper : ISocket
    {

        public ThreadSafeQueue<byte[]> Queue {get;set;}

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<ISocket> _onConnected;
        private Action<string, ISocket> _onMessage;
        private Action<ISocket> _onDisconnected;

        public WebSocketWrapper(string uri)
        {
            Queue = new ThreadSafeQueue<byte[]>();

            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;

            SendQueued();
        }

        protected void SendQueued()
        {
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

        public static ISocket Create(string uri)
        {
            return new WebSocketWrapper(uri);
        }

        public async Task<ISocket> Connect()
        {
            await ConnectAsync();
            return this;
        }

        public async Task<ISocket> Close()
        {
            await this._ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            return this;
        }
        public ISocket OnConnect(Action<ISocket> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        public ISocket OnDisconnect(Action<ISocket> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        public ISocket OnMessage(Action<string, ISocket> onMessage)
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
            await Task.Run(action);
        }
    }
}