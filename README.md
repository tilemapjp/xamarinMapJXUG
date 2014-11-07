xamarinMapJXUG
==============

Xamarin Google Maps SDK Sample in Japan Xamarin User Group Conference 西日本編

このサンプルはJapan Xamarin User Group Conference 西日本編での発表をベースにしたサンプルです。

Japan Xamarin User Group Conference 西日本編については https://atnd.org/events/57075 、発表のスライドについては http://www.slideshare.net/kokogiko/jxug2014-osaka-3-2 を参照してください。

サンプルの内容
---------------
TMSでの地図タイル取得、MBTilesでの地図タイル取得、TMS+MBTilesでの地図タイル取得の3種のサンプルが含まれています。
それぞれ、JXUGTMS、JXUGMBTiles、JXUGTMSMBTilesの名前空間で作成しています。

Google Maps API/SDKのAPIキー取得、置き換え
---------------
Google API Console https://code.google.com/apis/console/ より、Google Maps API/SDKのキーを取得してください。

iOS、Androidは別のキーになり、また共にアプリケーションの名前空間(iOSアプリでいうところのBundle Identifier相当のもの)が必要になります。  
サンプルにはiOS、Androidともに「jp.tilemap.名前空間」という名前空間が設定されていますが、適宜自分のものに変更してください。  
名前空間を変更するファイルは、Androidでは 名前空間.Droid/Properties/AndroidManifest.xml 、iOSでは 名前空間.Touch/Info.plist 内の設定になります。

また、Androidでは名前空間だけでなく、署名もAPIキーの要請に必要になります。

APIキーを入手した後は、プログラム内の適当な位置にAPIキーを記述してください。  
Androidでは 名前空間.Droid/Properties/AndroidManifest.xml 、iOSでは 名前空間.Touch/AppDelegate.cs になります。
共に、サンプルDL時は「PUT YOUR API KEY」となっているところを、APIキーで書き換えてください。

