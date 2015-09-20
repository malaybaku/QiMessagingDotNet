using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace Baku.QiMessaging
{

    /// <summary>JavascriptのQiSessionに相当する機能を提供するクラスを表します。</summary>
    public class QiSession : IQiSession
    {
        /// <summary>
        /// 指定された接続先を用いてセッションを初期化します。
        /// </summary>
        /// <param name="host">接続先(例: "http://192.168.1.10")</param>
        /// <param name="resources">接続時についでに指定するパラメタ(不要)</param>
        public QiSession(string host, JObject resources = null)
        {
            HostName = host;

            _socket = IO.Socket(host);
            InitializeSocket();
            _socket.Connect();
        }

        /// <summary>通信先を表すホスト名を取得します。</summary>
        public string HostName { get; }

        /// <summary>オブジェクトが破棄済みであるかどうかを取得します。</summary>
        public bool IsDisposed { get; private set; }

        /// <summary>モジュール名を指定してモジュールをロードします。</summary>
        /// <param name="serviceName">モジュール名</param>
        /// <param name="args">モジュールロード時のオプション(使っていません)</param>
        /// <returns>ロードされたモジュール</returns>
        public async Task<QiServiceModule> LoadService(string serviceName, params object[] args)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(QiSession));

            int id = PublishPostId();
            QiServiceModule result = null;
            JToken errorData = new JObject();

            bool success = false;
            bool disconnected = false;

            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;

            //コードの通りだが、成功と失敗のイベントを監視しつつ通信を試す
            using (var _ = Observable.FromEventPattern<QiSessionReplyEventArgs>(this, nameof(ReplyReceived))
                .FirstAsync(ep => ep.EventArgs.Id == id)
                .Subscribe(ep =>
                {
                    success = true;
                    result = QiServiceModule.CreateFromMetaObject(this, ep.EventArgs.Data);
                    cts.Cancel();
                }))
            using (var __ = Observable.FromEventPattern<QiSessionReplyEventArgs>(this, nameof(ErrorReceived))
                .FirstAsync(ep => ep.EventArgs.Id == id)
                .Subscribe(ep =>
                {
                    success = false;
                    errorData = ep.EventArgs.Data;
                    cts.Cancel();
                }))
            using (var ___ = Observable.FromEventPattern<EventArgs>(this, nameof(Disconnected))
                .Take(1)
                .Subscribe(____ =>
                {
                    success = false;
                    disconnected = true;
                    cts.Cancel();
                }))
            {

                Post(id, "ServiceDirectory", "service", new JArray(new JValue(serviceName)));
                await Task.Delay(Timeout.Infinite, token);

                if (disconnected)
                {
                    throw new QiSessionDisconnectedException();
                }

                if(!success)
                {
                    throw new QiSessionLoadServiceFailedException(id, serviceName, errorData);
                }

                return result;

            }


        }

        /// <summary>関数を呼び出します。</summary>
        /// <param name="objName">関数の呼び出し元を表す名前</param>
        /// <param name="methodName">関数の名前</param>
        /// <param name="arg">関数の引数</param>
        /// <returns>関数の戻り値</returns>
        public async Task<JToken> CallFunction(string objName, string methodName, JArray arg)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(QiSession));

            int id = PublishPostId();
            JToken result = new JObject();
            JToken errorData = new JObject();

            bool success = false;
            bool disconnected = false;

            CancellationTokenSource cts = new CancellationTokenSource();
            var token = cts.Token;

            //コードの通りだが、成功と失敗のイベントを監視しつつ通信を試す
            using (var _ = Observable.FromEventPattern<QiSessionReplyEventArgs>(this, nameof(ReplyReceived))
                .Where(ep => ep.EventArgs.Id == id)
                .Take(1)
                .Subscribe(ep =>
                {
                    success = true;
                    result = ep.EventArgs.Data;
                    cts.Cancel();
                }))
            using (var __ = Observable.FromEventPattern<QiSessionReplyEventArgs>(this, nameof(ErrorReceived))
                .Where(ep => ep.EventArgs.Id == id)
                .Take(1)
                .Subscribe(ep =>
                {
                    success = false;
                    errorData = ep.EventArgs.Data;
                    cts.Cancel();
                }))
            using (var ___ = Observable.FromEventPattern<EventArgs>(this, nameof(Disconnected))
                .Take(1)
                .Subscribe(____ =>
                {
                    success = false;
                    disconnected = true;
                    cts.Cancel();
                }))
            {

                Post(id, objName, methodName, arg);
                await Task.Delay(Timeout.Infinite, token);

                if (disconnected)
                {
                    throw new QiSessionDisconnectedException();
                }

                if (!success)
                {
                    throw new QiSessionCallFunctionFailedException(
                        id,
                        objName,
                        methodName,
                        arg,
                        errorData
                        );
                }

                return result;
            }

        }
        
        /// <summary>オブジェクトのイベント監視処理を宣言し、サーバからコールバック関数への割り当て名を取得します。</summary>
        /// <param name="objName">イベントの発行元であるオブジェクト名</param>
        /// <param name="signalName">イベント名</param>
        /// <returns>コールバックへ関連づけられた名前</returns>
        public async Task<string> RegisterEvent(string objName, string signalName)
        {
            var asFunctionResult = await CallFunction(objName, "registerEvent", new JArray(signalName));
            return (string)((asFunctionResult["result"] as JValue).Value);
        }

        /// <summary>サーバーにイベント購読解除を宣言します。</summary>
        /// <param name="objname">イベントの発行元</param>
        /// <param name="signalName">イベント名</param>
        /// <param name="linkName">購読時に割り当てられたコールバックの名称</param>
        /// <returns></returns>
        public async Task UnregisterEvent(string objName, string signalName, string linkName)
        {
            await CallFunction(objName, "unregisterEvent", new JArray(signalName, linkName));
        }

        /// <summary>リソースを解放します。</summary>
        public void Dispose() 
        {
            if(!IsDisposed)
            {
                _socket.Disconnect();
                IsDisposed = true;
            }
        }

        /// <summary>サーバへ"call"データを送信すると発生します。</summary>
        public event EventHandler<QiSessionDataSendEventArgs> DataSend;

        /// <summary>サーバから"reply"データを取得すると発生します。</summary>
        public event EventHandler<QiSessionReplyEventArgs> ReplyReceived;
     
        /// <summary>サーバから"error"データを取得すると発生します。</summary>
        public event EventHandler<QiSessionErrorEventArgs> ErrorReceived;
        
        /// <summary>サーバから"signal"データを取得すると発生します。</summary>
        public event EventHandler<QiSessionSignalEventArgs> SignalReceived;
        
        /// <summary>サーバから"disconnect"データを取得すると発生します。</summary>
        public event EventHandler Disconnected;

        #region プライベート実装

        private Socket _socket;
        private int TotalId = 0;

        /// <summary>送信元の名前、関数名、引数を指定してサーバへソケットデータを送信します。</summary>
        /// <param name="obj">送信元オブジェクトの名前</param>
        /// <param name="method">呼び出したい関数の名前</param>
        /// <param name="arg">関数の引数一覧</param>
        /// <returns>投げた通信に対応する一意ID</returns>
        private void Post(int id, string obj, string method, JArray arg)
        {
            var jobj = new JObject(
                new JProperty("idm", id),
                new JProperty("params",
                    new JObject(
                        new JProperty("obj", obj),
                        new JProperty("method", method),
                        new JProperty("args", arg)
                        )
                    )
                );

            _socket.Emit("call", jobj);

            DataSend?.Invoke(this, new QiSessionDataSendEventArgs(id, jobj));
        }

        /// <summary>ソケットのデータ受信方法を初期化します。</summary>
        private void InitializeSocket()
        {
            _socket.On("reply", data => ReplyReceived?.Invoke(this, new QiSessionReplyEventArgs((JObject)data)));

            _socket.On("error", data => ErrorReceived?.Invoke(this, new QiSessionErrorEventArgs((JObject)data)));

            _socket.On("disconnect", data => Disconnected?.Invoke(this, EventArgs.Empty));

            _socket.On("signal", data =>
            {
                //データの中身は文字列+イベントデータなのでそのまま拾って投げる
                var res = (data as JObject)["result"];
                SignalReceived?.Invoke(this, new QiSessionSignalEventArgs(
                    (string)((res["obj"] as JValue).Value),
                    (string)((res["signal"] as JValue).Value),
                    (string)((res["link"] as JValue).Value),
                    res["data"]
                    ));
            });
        }

        private object publishPostIdLock = new Object();
        /// <summary>通信に使える一意識別子となる整数を発行します。</summary>
        /// <returns>一意識別子になる整数</returns>
        private int PublishPostId()
        {
            lock (publishPostIdLock)
            {
                TotalId++;
                return TotalId;
            }
        }

        #endregion

    }


}
