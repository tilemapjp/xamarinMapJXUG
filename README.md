xamarinGMS
==========

Xamarin Google Maps SDK Sample in Geomedia Summit OSAKA 2013

このサンプルはジオメディアサミット2013大阪懇親会会場でのライトニングトークをベースにしたサンプルです。

ジオメディアサミット2013大阪については http://atnd.org/event/gmsosaka2013 、ライトニングトークのスライドについては http://www.slideshare.net/kokogiko/gms2013-xamarin-googlemapssdk を参照してください。

Xamarinの入手
---------------

詳細については割愛しますが、 http://xamarin.com/ からXamarin Studioを入手してください。  
その後、本githubのxamarinGMSリポジトリをチェックアウトし、Xamarin StudioでGeomediaSummit.slnを起動してください。  
利用しているライブラリの規模等の問題により、Starterエディションモードではコンパイルできません。1ヶ月のTrialモード(或いは有償のモード)で実行ください。

Google Play Serviceコンポーネントの導入 (Android)
---------------
amay077 さんの http://qiita.com/items/2d76a090d49926805431iOS こちらの記事に従って、Google Play servicesをXamarin上でセットアップします。  
その後、GeomediaSummitソリューション中のGooglePlayServicesプロジェクトはリンク切れになっていると思いますが、これを削除し、代わりに
amay077さん記事中のMapsAndLocationDemoソリューションに含まれている、GooglePlayServicesプロジェクトを追加してやります。  
また、GeomediaSummit.ViewDroidプロジェクトの参照フォルダを右クリックし、「参照アセンブリの編集」=>「Projectsタブ」で、追加したGooglePlayServicesへの参照を追加してやります。

GoogleMapsコンポーネントの追加 (iOS)
---------------
iOSでは、複雑な手順は必要なく、XamarinStoreというコンポーネントストアからGoogleMapsコンポーネントを無料で追加できます。  
GeomediaSummitソリューション中のGeomediaSummit.ViewTouchプロジェクトでは、Componentsフォルダ中のGoogleMapsコンポーネントがリンク切れになっていると思いますので、これを削除します。  
その後、Componentsフォルダをダブルクリックすると、Components管理のUIが出てきますので、その右上の「Get more components」よりXamarinStoreに行く事ができます。  
検索等からGoogleMapsコンポーネントを探し、Installすれば完了です。

Google Maps API/SDKのAPIキー取得、置き換え
---------------
Google API Console https://code.google.com/apis/console/ より、Google Maps API/SDKのキーを取得してください。

iOS、Androidは別のキーになり、また共にアプリケーションの名前空間(iOSアプリでいうところのBundle Identifier相当のもの)が必要になります。  
サンプルにはiOS、Androidともに「com.geomedia.summit」という名前空間が設定されていますが、適宜自分のものに偏向してください。  
名前空間を変更するファイルは、Androidでは GeomediaSummit.ViewDroid/Properties/AndroidManifest.xml 、iOSでは GeomediaSummit.ViewTouch/Info.plist 内の設定になります。

また、Androidでは名前空間だけでなく、署名もAPIキーの要請に必要になりますが、先に紹介した amay066 さんの記事ではAPIキーの取得まで解説されていますので、併せてご覧下さい。

APIキーを入手した後は、プログラム内の適当な位置にAPIキーを記述してください。  
Androidでは GeomediaSummit.ViewDroid/Properties/AndroidManifest.xml 、iOSでは GeomediaSummit.ViewTouch/AppDelegate.cs になります。
共に、サンプルDL時は「Your API Key Here」となっているところを、APIキーで書き換えてください。

