using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Google.Maps;
using MonoTouch.CoreLocation;

namespace JXUGTMS
{
	//iOS-TMS版：メイン画面
	public partial class MainViewController : UIViewController
	{
		private MapView       mapView;
		private URLConstructor constructor;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public MainViewController () : base()
		{
			constructor = new URLConstructor ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void LoadView ()
		{
			//MapViewはnewしてメインビューに
			mapView = new MapView ();
			View = mapView;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			//iOS TMS版はTileProviderクラスは不要：UrlTileLayer.FromUrlConstructorから無名関数で生成できる
			var tmsProvider = UrlTileLayer.FromUrlConstructor ((uint x, uint y, uint zoom) => {
				string url = constructor.GetUrl((int)x,(int)y,(int)zoom);
				return new NSUrl(url); 
			});
				
			var zoomLatLng = constructor.zoomLatLng ();

			//UrlTileLayerオブジェクトのMapプロパティにMapViewを与える
			tmsProvider.Map = mapView; 

			CameraUpdate update = CameraUpdate.SetCamera (
				new CameraPosition(new CLLocationCoordinate2D(zoomLatLng[2],zoomLatLng[1]),
			                   (float)zoomLatLng[0],0.0,0.0)
			);

			mapView.MoveCamera(update);
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (UserInterfaceIdiomIsPhone) {
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} else {
				return true;
			}
		}
	}
}

