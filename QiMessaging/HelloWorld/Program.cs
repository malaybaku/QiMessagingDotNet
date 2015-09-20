using System;
using System.IO;
using System.Threading.Tasks;

using Baku.QiMessaging;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "http://pepper1.local";
            Task.Run(async () =>
            {
                try
                {
                    using (var rawSession = new QiSession(host))
                    using (var writer = new StreamWriter("socket_datas.txt"))
                    using (var session = new LoggingQiSession(rawSession, writer))
                    {
                        var tts = await session.LoadService("ALTextToSpeech");
                        await tts.Invoke("setLanguage", "English");
                        await tts.Invoke("say", "Hello.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }).Wait();
        }
    }
}
