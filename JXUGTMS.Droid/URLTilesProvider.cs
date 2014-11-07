using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Gms;
using Android.Gms.Maps.Model;
using Android.Support.V4.App;

namespace JXUGTMS
{
	//Android-TMS版：タイルプロバイダー
	class URLTilesProvider : UrlTileProvider //この継承が必要
	{
		//Sharedプロジェクトから
		private URLConstructor constructor;

		private static int TILE_WIDTH  = 256;
		private static int TILE_HEIGHT = 256;

		public URLTilesProvider () : base (TILE_WIDTH, TILE_HEIGHT)
		{
			constructor = new URLConstructor ();
		}

		public double[] bounds()
		{
			return constructor.bounds ();
		}

		public double[] zoomLatLng()
		{
			return constructor.zoomLatLng ();
		}

		//Tile画像URLを返す為のメソッド(オーバーライド)
		public override Java.Net.URL GetTileUrl (int x, int y, int z)
		{
			//SharedプロジェクトからURL文字列を得る
			var url = constructor.GetUrl (x, y, z);
			return new Java.Net.URL (url);
		}
	}
}

