using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using System.IO;

namespace JXUGTMSMBTiles 
{
	//Android-TMS&MBTiles版：タイルプロバイダー(内部処理用)
	public class TMSMBTilesProviderInternal : UrlTileProvider
	{
		//これのGetTileがオーバーライドできれば、話は速いのですが…virtualではなくオーバーライドできないのでデコレーション
		public static int TILE_WIDTH  = 256;
		public static int TILE_HEIGHT = 256;

		protected TMSMBTilesAccessor accessor;

		public TMSMBTilesProviderInternal (TMSMBTilesAccessor accessor) : base(TILE_WIDTH,TILE_HEIGHT)
		{
			this.accessor = accessor;
		}

		//Tile画像URLを返す為のメソッド(オーバーライド)
		public override Java.Net.URL GetTileUrl (int x, int y, int z)
		{
			//SharedプロジェクトからURL文字列を得る
			var url = accessor.GetUrl (x, y, z);
			return new Java.Net.URL (url);
		}
	}

	//Android-TMS&MBTiles版：タイルプロバイダー(デコレーションクラス)
	public class TMSMBTilesProvider : Java.Lang.Object, ITileProvider
	{
		protected TMSMBTilesProviderInternal intern;
		protected TMSMBTilesAccessor accessor;

		public TMSMBTilesProvider () : base()
		{
			this.accessor = new TMSMBTilesAccessor();
			//デコレーションする内部オブジェクト
			intern = new TMSMBTilesProviderInternal (this.accessor);
		}

		//Tile画像を返す為のメソッド(ITileProviderで定義)
		public Tile GetTile (int x, int y, int z)
		{
			//ズームがタイルの最大ズーム以上の場合
			if (z > (int)accessor.maxzoom()) {
				var pow = Math.Pow (2, (double)z - accessor.maxzoom());
				//タイル最大ズームでのx,y算出
				var tileX = (int)(x / pow);
				var tileY = (int)(y / pow);

				//最大ズームでの画像取得
				var dzTile = this.GetTile (tileX, tileY, (int)accessor.maxzoom());

				//デジタルズームを実施後、Tileオブジェクトを生成して戻す
				var dzImage = new byte[dzTile.Data.Count];
				dzTile.Data.CopyTo (dzImage, 0);
				var dzBitmap = BitmapFactory.DecodeByteArray (dzImage, 0, dzImage.Length);

				var size = 256.0 / pow;
				var shiftX = (x - (uint)(tileX * pow)) * size;
				var shiftY = (y - (uint)(tileY * pow)) * size;
				var dzdBitmap = Bitmap.CreateBitmap (dzBitmap, (int)shiftX, (int)shiftY, (int)size, (int)size);

				byte[] dzdImage;
				using (var stream = new MemoryStream())
				{
					dzdBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
					dzdImage = stream.ToArray();
				}
					
				return new Tile (TMSMBTilesProviderInternal.TILE_WIDTH, TMSMBTilesProviderInternal.TILE_HEIGHT, dzdImage);
			}

			//SharedプロジェクトのMBTilesから、キャッシュがあるか確認
			var image = accessor.GetCachedTile (x, y, z);
			if (image != null) {
				//キャッシュが既にあれば、Tileオブジェクトを生成して戻す
				return new Tile (TMSMBTilesProviderInternal.TILE_WIDTH, TMSMBTilesProviderInternal.TILE_HEIGHT,
					image);
			}

			//UrlTileProviderにタイル要求
			var tile = intern.GetTile (x, y, z);

			if (tile != null && tile != TileProvider.NoTile && tile.Data != null) {
				//画像がnullでなければ、SharedプロジェクトのMBTilesにキャッシュ書き込み
				image = new byte[tile.Data.Count];
				tile.Data.CopyTo (image, 0);
				accessor.SetCachedTile (x, y, z, image);
			}

			return tile;
		}

		public double[] zoomLatLng()
		{
			return accessor.zoomLatLng ();
		}
	}
}