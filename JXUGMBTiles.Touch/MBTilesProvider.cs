using System;

using Google.Maps;
using MonoTouch.UIKit;

using MonoTouch.Foundation;

namespace JXUGMBTiles
{
	//iOS-MBTiles版：タイルプロバイダー
	public class MBTilesProvider : SyncTileLayer //この継承が必要
	{
		//Sharedプロジェクトから
		private MBTilesAccessor accessor;

		public MBTilesProvider () : base()
		{
			accessor = new MBTilesAccessor ();
		}

		public double[] bounds()
		{
			return accessor.bounds ();
		}

		public double[] zoomLatLng()
		{
			return accessor.zoomLatLng ();
		}

		//Tile画像を返す為のメソッド(オーバーライド)
		public override UIImage Tile (uint x, uint y, uint zoom)
		{
			//Sharedプロジェクトから画像イメージをバイト列で得る
			var image = accessor.GetTileImage ((int)x, (int)y, (int)zoom);

			if (image == null) {
				return Constants.TileLayerNoTile;
			} else {
				return UIImage.LoadFromData (NSData.FromArray(image));
			}
		}
	}
}

