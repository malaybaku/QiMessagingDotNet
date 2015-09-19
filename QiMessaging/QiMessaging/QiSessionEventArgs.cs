using System;

using Newtonsoft.Json.Linq;

namespace Baku.QiMessaging
{
    /// <summary>"reply"の受信データを表します。</summary>
    public class QiSessionReplyEventArgs : EventArgs
    {
        public QiSessionReplyEventArgs(JObject data)
        {
            Id = (int)((data["idm"] as JValue).Value);
            Data = data;
        }

        /// <summary>対応するポストを一意に指定するId</summary>
        public int Id { get; }

        /// <summary>リモートから受け取ったJSONデータの全体</summary>
        public JObject Data { get; }

    }

    /// <summary>"error"の受信データを表します。</summary>
    public class QiSessionErrorEventArgs : EventArgs
    {
        public QiSessionErrorEventArgs(JObject data)
        {
            Id = (int)((data["idm"] as JValue).Value);
            Data = data;
        }

        /// <summary>対応するポストを一意に指定するId</summary>
        public int Id { get; }

        /// <summary>リモートから受け取ったJSONデータの全体</summary>
        public JObject Data { get; }

    }

    /// <summary>"signal"の受信データを表します。</summary>
    public class QiSessionSignalEventArgs : EventArgs
    {
        public QiSessionSignalEventArgs(string objectName, string signalName, string linkName, JToken data)
        {
            ObjectName = objectName;
            SignalName = signalName;
            LinkName = linkName;
            Data = data;
        }

        /// <summary>シグナルの発行元オブジェクト名</summary>
        public string ObjectName { get; }

        /// <summary>シグナル名</summary>
        public string SignalName { get; }

        /// <summary>シグナルに関連づけられたリンク名</summary>
        public string LinkName { get; }

        /// <summary>シグナルに関連づけられたJSONデータ</summary>
        public JToken Data { get; }
    }
}
