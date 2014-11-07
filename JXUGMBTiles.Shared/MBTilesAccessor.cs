using System;
using System.Reflection;

using System.IO;
using System.Data;
//using Mono.Data.Sqlite;
using SQLite;

namespace JXUGMBTiles
{
	//SQLite.net MBTilesのスキーマ
	public class tiles
	{
		public int zoom_level   { get; set; }

		public int tile_column  { get; set; }

		public int tile_row     { get; set; }

		public byte[] tile_data { get; set; }
	}

	public class metadata
	{
		public string name  { get; set; }

		public string value { get; set; }
	}

	//MBTiles版：Sharedプロジェクト
	public partial class MBTilesAccessor
	{
		private string dbFile = "suita.mbtiles";
		private string dbFolder;
		private string dbPath;

		public MBTilesAccessor ()
		{
			//DBファイルの準備
			this.Initialize();
		}

		private void Initialize ()
		{
			//クロスプラットフォームで、保存先のフォルダを得る
			this.dbFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			this.dbPath   = Path.Combine (this.dbFolder, this.dbFile);

			//DBファイルがまだなければ、コピー
			if (!File.Exists (this.dbPath)) {
				if (!Directory.Exists (this.dbFolder)) {
					Directory.CreateDirectory (this.dbFolder);
				}

				//クロスプラットフォームで、EmbededResourceのStreamを読み込みオープンする
				Type type = this.GetType (); 
				Stream myInput  = type.Assembly.GetManifestResourceStream ("JXUGMBTiles." + this.dbFile);
				//保存先を書き込みオープンする
				Stream myOutput = new FileStream(this.dbPath, FileMode.OpenOrCreate);

				//内容コピー
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

		public double[] zoomLatLng()
		{
			double[] zoomLatLng = new double[3];

			using (var conn = new SQLiteConnection(this.dbPath)) {
				try {
					var query = conn.Table<metadata> ().Where (v => v.name == "center");

					if (query.Count () != 0) {
						var meta = query.First ();
						var value = meta.value;
						var valArray = value.Split (',');

						zoomLatLng[0] = double.Parse(valArray [2]);
						for (var i = 0; i < 2; i++) {
							zoomLatLng [i + 1] = double.Parse(valArray [i]);
						}
					}
				} catch (Exception ex) {
				}
			}
			return zoomLatLng;
		}

		public double minzoom()
		{
			double minzoom = 0.0;

			using (var conn = new SQLiteConnection(this.dbPath)) {
				try {
					var query = conn.Table<metadata> ().Where (v => v.name == "minzoom");

					if (query.Count () != 0) {
						var meta = query.First ();
						var value = meta.value;
						minzoom = double.Parse (value);
					}
				} catch (Exception ex) {
				}
			}
			return minzoom;
		}

		public double maxzoom()
		{
			double maxzoom = 0.0;

			using (var conn = new SQLiteConnection(this.dbPath)) {
				try {
					var query = conn.Table<metadata> ().Where (v => v.name == "maxzoom");

					if (query.Count () != 0) {
						var meta = query.First ();
						var value = meta.value;
						maxzoom = double.Parse (value);
					}
				} catch (Exception ex) {
				}
			}
			return maxzoom;
		}

		public double[] bounds()
		{
			double[] bounds = new double[4];

			using (var conn = new SQLiteConnection(this.dbPath)) {
				try {
					var query = conn.Table<metadata> ().Where (v => v.name == "bounds");

					if (query.Count () != 0) {
						var meta = query.First ();
						var value = meta.value;
						var valArray = value.Split (',');

						for (var i = 0; i < 4; i++) {
							bounds [i] = double.Parse(valArray [i]);
						}
					}
				} catch (Exception ex) {
				}
			}

			return bounds;
		}

		//x,y,zから、画像イメージをバイト列で返す
		public byte[] GetTileImage(int x, int y, int z)
		{
			byte[] image = null;

			//TMS形式はGoogleのY座標と正負の方向が逆なので直す
			y = (int)Math.Pow (2.0, (double)z) - y - 1;

			//SQLite.NETで、x,y,zから対応画像を検索
			using (var conn = new SQLiteConnection(this.dbPath)) {
				try {
					var query = conn.Table<tiles> ().Where (v => v.zoom_level == z && v.tile_column == x && v.tile_row == y);

					if (query.Count () != 0) {
						var tile = query.First ();
						image = tile.tile_data;
					}
				} catch (Exception ex) {
					image = null;
				}
			}

			return image;
		}
	}
}

