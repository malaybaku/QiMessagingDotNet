using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Baku.QiMessaging
{
    /// <summary>通信データのロギング機能をIQiSessionに追加します。</summary>
    public class LoggingQiSession : IQiSession
    {
        public LoggingQiSession(IQiSession session, TextWriter writer)
        {
            _session = session;
            _writer = writer;
        }

        private readonly IQiSession _session;
        private readonly TextWriter _writer;

        #region ラップしてるだけのインターフェース実装
        public string HostName => _session.HostName;

        public event EventHandler<QiSessionDataSendEventArgs> DataSend
        {
            add { _session.DataSend += value; }
            remove { _session.DataSend -= value; }
        }
        public event EventHandler Disconnected
        {
            add { _session.Disconnected += value; }
            remove { _session.Disconnected -= value; }
        }
        public event EventHandler<QiSessionErrorEventArgs> ErrorReceived
        {
            add { _session.ErrorReceived += value; }
            remove { _session.ErrorReceived -= value; }
        }
        public event EventHandler<QiSessionReplyEventArgs> ReplyReceived
        {
            add { _session.ReplyReceived += value; }
            remove { _session.ReplyReceived -= value; }
        }
        public event EventHandler<QiSessionSignalEventArgs> SignalReceived
        {
            add { _session.SignalReceived += value; }
            remove { _session.SignalReceived -= value; }
        }

        public Task<JToken> CallFunction(string objName, string methodName, JArray arg)
            => _session.CallFunction(objName, methodName, arg);

        public Task<QiServiceModule> LoadService(string serviceName, params object[] args)
            => _session.LoadService(serviceName, args);

        public Task<string> RegisterEvent(string objName, string signalName)
            => _session.RegisterEvent(objName, signalName);

        public Task UnregisterEvent(string objName, string signalName, string linkName)
            => _session.UnregisterEvent(objName, signalName, linkName);
        #endregion

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _session.Dispose();
                _writer.Dispose();
                IsDisposed = true;
            }
        }

    }
}
