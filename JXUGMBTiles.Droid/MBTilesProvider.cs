using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using System.IO;

namespace JXUGMBTiles
{
	//Android-MBTiles版：タイルプロバイダー
	class MBTilesProvider : Java.Lang.Object, ITileProvider //この2つの継承が必要
	{
		//Sharedプロジェクトから
		private MBTilesAccessor accessor;

		private static int TILE_WIDTH  = 256;
		private static int TILE_HEIGHT = 256;

		public MBTilesProvider () : base ()
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

		//Tile画像を返す為のメソッド(ITileProviderで定義)
		public Tile GetTile(int x, int y, int zoom) 
		{
			//Sharedプロジェクトから画像イメージをバイト列で得る
			var image = accessor.GetTileImage (x, y, zoom);

			if (image == null) {
				return TileProvider.NoTile;
			} else {
				return new Tile(TILE_WIDTH, TILE_HEIGHT, image);
			}
		}
	}
}

