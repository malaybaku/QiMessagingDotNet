using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Baku.QiMessaging
{

    /// <summary>QiMessageで送られてくるメソッド/シグナル情報を格納し直したクラス</summary>
    public class QiServiceModule
    {
        private QiServiceModule(
            string name, 
            QiSession session, 
            IEnumerable<string> methodNames, 
            IEnumerable<string> signalNames
            )
        {
            Name = name;
            Session = session;
            MethodNames = methodNames.ToArray();
            SignalNames = signalNames.ToArray();
        }

        /// <summary>このモジュールにサーバから割り当てられた名前を取得します。</summary>
        public string Name { get; }
        
        /// <summary>このモジュールをロードした元のセッションを取得します。</summary>
        public QiSession Session { get; }

        /// <summary>このモジュールから呼び出し可能な関数名の一覧を取得します。</summary>
        public IReadOnlyCollection<string> MethodNames { get; }

        /// <summary>このモジュールで購読可能なシグナル名の一覧を取得します。</summary>
        public IReadOnlyCollection<string> SignalNames { get; }


        /// <summary>
        /// 名前ベースで関数を呼び出します。該当名の有無をチェックしません。
        /// </summary>
        /// <param name="methodname">メソッド名</param>
        /// <param name="args">引数</param>
        /// <returns>呼び出した関数の戻り値</returns>
        public async Task<JToken> Invoke(string methodName, params object[] arg)
        {
            return await Session.CallFunction(Name, methodName, new JArray(arg));
        }

        /// <summary>
        /// 名前ベースで関数を呼び出します。
        /// 該当名が無い場合は空のJObjectを投げ返します。
        /// </summary>
        /// <param name="methodname">メソッド名</param>
        /// <param name="args">引数</param>
        /// <returns></returns>
        public async Task<JToken> TryInvoke(string methodName, params object[] arg)
        {
            if(MethodNames.Contains(methodName))
            {
                return await Session.CallFunction(Name, methodName, new JArray(arg));
            }
            else
            {
                return new JObject();
            }
        }

        /// <summary>イベントを購読します。</summary>
        /// <param name="signalName">シグナル名</param>
        /// <returns>イベントの発行元</returns>
        public async Task<IObservable<JToken>> ObserveSignal(string signalName)
        {
            //見ての通り、指定されたイベントだけフィルタリングする(パフォーマンスは若干悪いかもね)
            string allocName = await Session.RegisterEvent(this.Name, signalName);
            return Observable.FromEventPattern<QiSessionSignalEventArgs>(Session, nameof(Session.SignalReceived))
                .Where(ep => ep.EventArgs.ObjectName == Name &&
                    ep.EventArgs.SignalName == signalName &&
                    ep.EventArgs.LinkName == allocName)
                .Select(ep => ep.EventArgs.Data);
        }

        /// <summary>イベントを購読します。シグナル名を間違えた場合はイベントが発生しません。</summary>
        /// <param name="signalName">シグナル名</param>
        /// <returns>イベントの発行元</returns>
        public async Task<IObservable<JToken>> TryObserveSignal(string signalName)
        {
            if(SignalNames.Contains(signalName))
            {
                return await ObserveSignal(signalName);
            }
            else
            {
                return Observable.Empty<JToken>();
            }
        }

        /// <summary>メソッド名やシグナル名の情報をもとにインスタンスを生成します。</summary>
        /// <param name="data">メソッド/シグナル名情報を含むサーバからのデータ全体</param>
        /// <returns>新しいインスタンス</returns>
        public static QiServiceModule CreateFromMetaObject(QiSession session, JObject data)
        {
            string name = (string)((data["result"]["pyobject"] as JValue).Value);

            var methodNames = (data["result"]["metaobject"]["methods"] as JObject)
                .Values()
                .Select(jt => (string)((jt as JValue).Value));

            var signalNames = (data["result"]["metaobject"]["signals"] as JObject)
                .Values()
                .Select(jt => (string)((jt as JValue).Value));

            return new QiServiceModule(name, session, methodNames, signalNames);
        }

    }


}
