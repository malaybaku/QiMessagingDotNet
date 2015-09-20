using System;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Baku.QiMessaging
{
    /// <summary>JavascriptのQiSessionに相当する機能を提供するクラスを表します。</summary>
    public interface IQiSession : IDisposable
    {
        /// <summary>通信先を表すホスト名を取得します。</summary>
        string HostName { get; }

        /// <summary>オブジェクトが破棄済みであるかどうかを取得します。</summary>
        bool IsDisposed { get; }

        /// <summary>モジュール名を指定してモジュールをロードします。</summary>
        /// <param name="serviceName">モジュール名</param>
        /// <param name="args">モジュールロード時のオプション(使っていません)</param>
        /// <returns>ロードされたモジュール</returns>
        Task<QiServiceModule> LoadService(string serviceName, params object[] args);

        /// <summary>関数を呼び出します。</summary>
        /// <param name="objName">関数の呼び出し元を表す名前</param>
        /// <param name="methodName">関数の名前</param>
        /// <param name="arg">関数の引数</param>
        /// <returns>関数の戻り値</returns>
        Task<JToken> CallFunction(string objName, string methodName, JArray arg);

        /// <summary>オブジェクトのイベント監視処理を宣言し、サーバからコールバック関数への割り当て名を取得します。</summary>
        /// <param name="objName">イベントの発行元であるオブジェクト名</param>
        /// <param name="signalName">イベント名</param>
        /// <returns>コールバックへ関連づけられた名前</returns>
        Task<string> RegisterEvent(string objName, string signalName);

        /// <summary>サーバーにイベント購読解除を宣言します。</summary>
        /// <param name="objname">イベントの発行元</param>
        /// <param name="signalName">イベント名</param>
        /// <param name="linkName">購読時に割り当てられたコールバックの名称</param>
        /// <returns></returns>
        Task UnregisterEvent(string objName, string signalName, string linkName);

        /// <summary>サーバへ"call"データを送信すると発生します。</summary>
        event EventHandler<QiSessionDataSendEventArgs> DataSend;

        /// <summary>サーバから"reply"データを取得すると発生します。</summary>
        event EventHandler<QiSessionReplyEventArgs> ReplyReceived;

        /// <summary>サーバから"error"データを取得すると発生します。</summary>
        event EventHandler<QiSessionErrorEventArgs> ErrorReceived;

        /// <summary>サーバから"signal"データを取得すると発生します。</summary>
        event EventHandler<QiSessionSignalEventArgs> SignalReceived;

        /// <summary>サーバから"disconnect"データを取得すると発生します。</summary>
        event EventHandler Disconnected;

    }
}
