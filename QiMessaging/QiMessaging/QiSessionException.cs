using System;

using Newtonsoft.Json.Linq;

namespace Baku.QiMessaging
{

    /// <summary>モジュールのロード失敗を表します。</summary>
    public class QiSessionLoadServiceFailedException : Exception
    {
        public QiSessionLoadServiceFailedException(int id, string serviceName, JToken data)
            : base($"Failed to load service '{serviceName}'")
        {
            Id = id;
            ServiceName = serviceName;
            ErrorData = data;
        }

        public int Id { get; }
        public string ServiceName { get; }
        public JToken ErrorData { get; }

    }

    /// <summary>関数の呼び出し失敗を表します。</summary>
    public class QiSessionCallFunctionFailedException : Exception
    {
        public QiSessionCallFunctionFailedException(int id, string objectName, string methodName, JToken arg, JToken data)
            : base($"Failed to call {objectName}.{methodName}({arg})")
        {
            Id = id;
            ObjectName = objectName;
            MethodName = methodName;
            Arguments = arg;
            ErrorData = data;
        }

        public int Id { get; }
        public string ObjectName { get; }
        public string MethodName { get; }
        public JToken Arguments { get; }
        public JToken ErrorData { get; }

    }

    /// <summary>セッションが切断したことによって関数呼び出しに失敗したことを表します。</summary>
    public class QiSessionDisconnectedException : Exception
    {
        public QiSessionDisconnectedException()
            : base("QiSession disconnected")
        {

        }
    }

}
