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
                using (var session = new QiSession(host))
                {
                    var tts = await session.LoadService("ALTextToSpeech");
                    await tts.Invoke("setLanguage", "English");
                    await tts.Invoke("say", "Hello.");
                }
            }).Wait();
        }
    }
}
