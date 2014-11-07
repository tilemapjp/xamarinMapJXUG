using System;
using Google.Maps;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace JXUGTMSMBTiles 
{
	//iOS-TMS&MBTiles版：タイルプロバイダー
	public class TMSMBTilesProvider : TileLayer //タイル画像要求のコールバックを受け取るため、TileLayerを継承
	{
		protected UrlTileLayer intern;
		protected TMSMBTilesAccessor accessor;

		public TMSMBTilesProvider ()
		{
			this.accessor = new TMSMBTilesAccessor ();
			//内部でUrlTileLayerに画像取得処理を実施させるため、内部処理用オブジェクトを保持しておく
			intern = UrlTileLayer.FromUrlConstructor ((uint x, uint y, uint zoom) => {
				var url = accessor.GetUrl((int)x,(int)y,(int)zoom);
				return url == null ? null : new NSUrl(url); 
			});
		}

		//TileLayerを継承すると、Tile画像が必要な際にMapViewからこのメソッドが呼ばれる
		//画像は非同期で、引数ITileReceiverに返してやる
		public override void RequestTile (uint x, uint y, uint zoom, ITileReceiver receiver)
		{
			//ズームがタイルの最大ズーム以上の場合
			if (zoom > (uint)accessor.maxzoom()) {
				var deltaZ = (double)zoom - accessor.maxzoom();
				//タイル最大ズームでのx,y算出
				var tileX = (uint)(x / Math.Pow(2,deltaZ));
				var tileY = (uint)(y / Math.Pow(2,deltaZ));

				//オリジナルサイズの情報も渡して独自コールバックオブジェクト生成
				var wrappedOrig = new TMSMBTilesReceiver (receiver, accessor, x, y, zoom);

				//自分自身に、独自コールバックオブジェクトを与えてタイル要求
				//この時のtileX,tileYは、x,yと異なり最大ズームでの値になっている事に注意
				this.RequestTile (tileX, tileY, (uint)accessor.maxzoom(), wrappedOrig);
				return;
			}

			//SharedプロジェクトのMBTilesから、キャッシュがあるか確認
			var image = accessor.GetCachedTile ((int)x, (int)y, (int)zoom);
			if (image != null) {
				//キャッシュが既にあれば、ITileReceiverに返してやる
				receiver.ReceiveTile(x, y, zoom, new UIImage(NSData.FromArray(image)));
				return;
			}

			//ITileReceiverをラップして独自コールバックオブジェクト生成
			var wrapped = new TMSMBTilesReceiver (receiver, accessor);

			//UrlTileLayerに独自コールバックオブジェクトを渡してタイル要求
			intern.RequestTile (x, y, zoom, wrapped);
		}

		public double[] zoomLatLng()
		{
			return accessor.zoomLatLng ();
		}
	}

	//ITileReceiverを継承した独自コールバックオブジェクト
	//UrlTileLayerからの取得画像を受け取るのが主な役割
	public class TMSMBTilesReceiver : NSObject, ITileReceiver 
	{
		public ITileReceiver origin; //ラップする前のオリジナルITileReceiver
		private TMSMBTilesAccessor accessor;
		//オリジナルサイズ
		private uint originX = 0;
		private uint originY = 0;
		private uint originZoom = 0;

		//デジタルズームなしの場合のコンストラクタ
		public TMSMBTilesReceiver (ITileReceiver origin, TMSMBTilesAccessor accessor)
		{
			this.origin   = origin;
			this.accessor = accessor;
		}

		//デジタルズーム要の場合のオリジナルサイズ付きコンストラクタ
		public TMSMBTilesReceiver (ITileReceiver origin, TMSMBTilesAccessor accessor, 
			uint originX, uint originY, uint originZoom) : this(origin, accessor)
		{
			this.originX = originX;
			this.originY = originY;
			this.originZoom = originZoom;
		}

		//コールバックでの画像の受け取り
		public void ReceiveTile (uint x, uint y, uint zoom, UIImage image)
		{
			//デジタルズームが必要な際は、デジタルズームする
			if (this.originZoom != 0) {
				var pow = Math.Pow (2, this.originZoom - zoom);
				var size = 256.0 / pow;
				var shiftX = (this.originX - (uint)(x * pow)) * size;
				var shiftY = (this.originY - (uint)(y * pow)) * size;

				var rect = new RectangleF ((float)shiftX, (float)shiftY, (float)size, (float)size);
				using (CGImage cr = image.CGImage.WithImageInRect (rect)) {
					image = UIImage.FromImage (cr);
				}

				origin.ReceiveTile (this.originX, this.originY, this.originZoom, image);
				return;
			}

			//ラップする前のオリジナルITileReceiverに画像を返してやる
			origin.ReceiveTile (x, y, zoom, image);

			if (image != null) {
				//画像がnullでなければ、SharedプロジェクトのMBTilesにキャッシュ書き込み
				var data = image.AsPNG ();
				if ( data != null ) {
					var bytes = new byte[data.Length];

					System.Runtime.InteropServices.Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
					accessor.SetCachedTile ((int)x, (int)y, (int)zoom, bytes);
				}
			}
		}
	}
}