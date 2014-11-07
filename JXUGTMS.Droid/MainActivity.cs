using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Support;
using Android.Support.V4.App;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace JXUGTMS
{
	//Android-TMS版：メイン画面
	[Activity (Label = "JXUGTMS", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private GoogleMap mapView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
		}

		protected override void OnResume ()
		{
			if (mapView == null) {
				//MapViewはMapFragmentから
				mapView = ((MapFragment)FragmentManager.FindFragmentById (Resource.Id.map)).Map;

				if (mapView != null) {
					mapView.MyLocationEnabled = true;

					var tmsProvider = new URLTilesProvider ();
					var mbOptions  = new TileOverlayOptions();
					mapView.AddTileOverlay( mbOptions.InvokeTileProvider(tmsProvider) );
					//TileOverlayOptions#InvokeTileProvider にITileProviderを与えて、その結果をMapViewにAddTileOverlay

					var zoomLatLng = tmsProvider.zoomLatLng ();

					var update = CameraUpdateFactory.NewLatLngZoom (
						new LatLng (zoomLatLng[2], zoomLatLng [1]),
						(float)zoomLatLng [0]
					);

					mapView.MoveCamera(update);
				}
			}
			base.OnResume();
		}	
	
	}
}


