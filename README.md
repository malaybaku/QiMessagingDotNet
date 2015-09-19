## QiMessagingDotNet

### Copyright 獏(ばく) 2015

### 1. About/概要
This library is personal translation of qimessaging library by Aldebaran Robotics for .NET Framework.

このライブラリはアルデバラン社がロボットの通信用に整備している"qimessaging"ライブラリを.NET Framework向けに書き直したものです。個人的に製作したものであり、アルデバラン社が公開するもともとのライブラリ(libqi)とは直接関係ありません。


### 2. Example/使用例
"Hello World" project indicates how to use the library.

"Hello World"プロジェクトが使用方法の例になっています。

```
using System.Threading.Tasks;
using Baku.QiMessaging;

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
```

A basic workflow is as follows.

1. set host name by Robot's IP address with prefix "http://", and create QiSession
2. call "LoadService" function from QiSession instance, and get QiServiceModule
3. call "Invoke" function from QiServiceModule


基本的な使用手順は以下の通りです。

1. ロボットのIPアドレスに"http://"をつけたホスト名を指定してセッションを開始
2. セッションのインスタンスでLoadService関数を呼び、非同期形式でサービス(QiServiceModule)を取得
3. サービス上で"Invoke"関数を用いてメソッド名と関数引数を指定することで実際の処理を行う。



### 3. LICENSE/ライセンス
Source code is opened with MIT License as written in "LICENSE.txt". Also see "Third party libraries."

ソースコードはMITライセンス("LICENSE.txt")で公開しています。下の「使用している第三者ライブラリ」も合わせて確認してください。

注: プログラム全体を作動させる場合、私自身が課すMITライセンスに加え、下記の「仕様している第三者ライブラリ」にあるようにMITライセンスとApache 2.0ライセンスの順守が必須となります。


### 4. Third Party Libraries/使用している第三者ライブラリ

#### 4-1. SocketIoClientDotNet
web page(NuGet): https://www.nuget.org/packages/SocketIoClientDotNet/
web page(GitHub): https://github.com/Quobject/SocketIoClientDotNet
license(MIT): https://github.com/Quobject/SocketIoClientDotNet/blob/master/LICENSE.md

#### 4-2. EngineIoClientDotNet
web page(GitHub): https://github.com/Quobject/EngineIoClientDotNet
license(MIT): https://github.com/Quobject/EngineIoClientDotNet/blob/master/LICENSE

#### 4-3. WebSocket4Net
web page(CodePlex): https://websocket4net.codeplex.com/
license(Apache 2.0): https://websocket4net.codeplex.com/license

#### 4-4. Newtonsoft.Json
web page(NuGet): https://www.nuget.org/packages/Newtonsoft.Json/
license(MIT): https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md



### 5. Reference/参考
Aldebaran Robotics opens QiMessaging source code in GitHub.

Aldebaran社はQiMessagingライブラリのソースコードをGitHubに公開しています。

- libqi: https://github.com/aldebaran/libqi
- libqi-js: https://github.com/aldebaran/libqi-js

本プログラムはjavascript向けのQiMessaging実装である"libqi-js"に近い挙動を行うことを目的としています。


### Contact/連絡
- e-mail:  njslyr.trpg[_at_]gmail.com
- twitter: https://twitter.com/baku_dreameater
