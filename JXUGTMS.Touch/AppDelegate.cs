using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Google.Maps;

namespace JXUGTMS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		MainViewController viewController;

		const string MapsApiKey = "PUT YOUR API KEY";
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			MapServices.ProvideAPIKey (MapsApiKey);

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new MainViewController ();
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();

			Console.WriteLine (NSBundle.MainBundle.BundlePath);
			
			return true;
		}
	}
}

