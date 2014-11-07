using System;

namespace JXUGTMS
{
	//TMS版：Sharedプロジェクト
	public partial class URLConstructor
	{
		//TMS元定義
		private string baseURL = "http://t.tilemap.jp/jcp_maps/harima/{z}/{x}/{y}.png";
		private string regexURL = null;

		public URLConstructor ()
		{
			//String.Formatで置き換えられる文字列に
			regexURL = baseURL.Replace("{x}", "{0}");
			regexURL = regexURL.Replace("{y}", "{1}");
			regexURL = regexURL.Replace("{z}", "{2}");
		}

		public double[] zoomLatLng()
		{
			double[] zoomLatLng = new double[3]{
				13,134.463654623301,34.7805578035837
			};

			return zoomLatLng;
		}

		public double minzoom()
		{
			return 11;
		}

		public double maxzoom()
		{
			return 16;
		}

		public double[] bounds()
		{
			double[] bounds = new double[4]{
				134.410868034551,34.7350834424502,134.516441212051,34.8260321647173
			};

			return bounds;
		}

		//x,y,zから、画像URLをStringで返す
		public string GetUrl(int x, int y, int z)
		{
			//TMS形式はGoogleのY座標と正負の方向が逆なので直す
			y = (int)Math.Pow (2.0, (double)z) - y - 1;

			//x,y,zをString.Formatに
			string url = String.Format (regexURL, x, y, z);
			return url;
		}
	}
}

