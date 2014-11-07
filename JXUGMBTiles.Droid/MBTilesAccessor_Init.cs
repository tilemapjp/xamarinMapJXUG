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
using System.IO;

namespace JXUGMBTiles
{
	/*public partial class MBTilesAccessor
	{
		public void Initialize ()
		{
			this.dbPath = Application.Context.GetDatabasePath(this.dbFile).AbsolutePath;
			this.dbFolder = System.IO.Path.GetDirectoryName(this.dbPath);

			if (!File.Exists(this.dbPath)) {
				if (!File.Exists(this.dbFolder))
					Directory.CreateDirectory(this.dbFolder);

				Stream myInput  = Application.Context.Assets.Open(this.dbFile);
				Stream myOutput = new FileStream(this.dbPath, FileMode.OpenOrCreate);

				byte[] buffer = new byte[1024];
				int b = buffer.Length;
				int length;
				while ((length = myInput.Read(buffer, 0, b)) > 0)
				{
					myOutput.Write(buffer, 0, length);
				}

				myOutput.Flush();
				myOutput.Close();
				myInput.Close();
			}
		}
	}*/
}

