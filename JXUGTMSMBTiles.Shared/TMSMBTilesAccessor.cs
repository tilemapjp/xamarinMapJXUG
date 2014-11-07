using System;

using System.IO;
using System.Data;
using SQLite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace JXUGTMSMBTiles
{
	//x,y,zをセットで保持するための構造体
	public struct XYZOOM
	{
		public int x;
		public int y;
		public int zoom;
	}

	//SQLite.net MBTilesのスキーマ (スキーマ生成するのでフルで)
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

	public class images
	{
		public byte[] tile_data { get; set; }

		public string tile_id   { get; set; }
	}

	public class map
	{
		public int zoom_level  { get; set; }

		public int tile_column { get; set; }

		public int tile_row    { get; set; }

		public string tile_id  { get; set; }

		public string grid_id  { get; set; }
	}

	public class TMSMBTilesAccessor
	{
		//MBTiles名、TMS元定義等
		private string dbFile = "shikama.mbtiles";
		private string baseURL = "http://t.tilemap.jp/jcp_maps/shikama/{z}/{x}/{y}.png";
		private string regexURL = null;
		private string dbFolder;
		private string dbPath;

		//MBTilesへのキャッシュセットが集中した時の際のロック用オブジェクト
		private object lockObj = new object();
		//MBTilesへのキャッシュセットが集中した時の際のキュー用オブジェクト
		private List<XYZOOM> cacheTileQueIndex = new List<XYZOOM> ();
		private Dictionary<XYZOOM, byte[]> cacheTileQueBag = new Dictionary<XYZOOM,byte[]> ();
		private bool cacheTileWorking = false;

		public TMSMBTilesAccessor ()
		{
			//DBファイルの準備
			this.Initialize();
		}

		private void Initialize() 
		{
			//String.Formatで置き換えられる文字列に
			regexURL = baseURL.Replace("{x}", "{0}");
			regexURL = regexURL.Replace("{y}", "{1}");
			regexURL = regexURL.Replace("{z}", "{2}");

			//クロスプラットフォームで、保存先のフォルダを得る
			this.dbFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			this.dbPath   = Path.Combine (this.dbFolder, this.dbFile);

			//DBファイルがまだなければ、作る
			if (!File.Exists(this.dbPath)) {
				if (!Directory.Exists (this.dbFolder)) 
				{
					Directory.CreateDirectory (this.dbFolder);
				}
				File.Create (this.dbPath).Close();

				//スキーマ生成
				using (var conn = new SQLiteConnection(this.dbPath)) {
					conn.Execute ("CREATE TABLE map ( zoom_level INTEGER, tile_column INTEGER, tile_row INTEGER, tile_id TEXT, grid_id TEXT );");
					conn.Execute ("CREATE TABLE grid_key ( grid_id TEXT, key_name TEXT );");
					conn.Execute ("CREATE TABLE keymap ( key_name TEXT, key_json TEXT );");
					conn.Execute ("CREATE TABLE grid_utfgrid ( grid_id TEXT, grid_utfgrid BLOB );");
					conn.Execute ("CREATE TABLE images ( tile_data blob, tile_id text );");
					conn.Execute ("CREATE TABLE metadata ( name text, value text );");
					conn.Execute ("CREATE UNIQUE INDEX map_index ON map (zoom_level, tile_column, tile_row);");
					conn.Execute ("CREATE UNIQUE INDEX grid_key_lookup ON grid_key (grid_id, key_name);");
					conn.Execute ("CREATE UNIQUE INDEX keymap_lookup ON keymap (key_name);");
					conn.Execute ("CREATE UNIQUE INDEX grid_utfgrid_lookup ON grid_utfgrid (grid_id);");
					conn.Execute ("CREATE UNIQUE INDEX images_id ON images (tile_id);");
					conn.Execute ("CREATE UNIQUE INDEX name ON metadata (name);");
					conn.Execute ("CREATE VIEW tiles AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, images.tile_data AS tile_data FROM map JOIN images ON images.tile_id = map.tile_id;");
					conn.Execute ("CREATE VIEW grids AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, grid_utfgrid.grid_utfgrid AS grid FROM map JOIN grid_utfgrid ON grid_utfgrid.grid_id = map.grid_id;");
					conn.Execute ("CREATE VIEW grid_data AS SELECT map.zoom_level AS zoom_level, map.tile_column AS tile_column, map.tile_row AS tile_row, keymap.key_name AS key_name, keymap.key_json AS key_json FROM map JOIN grid_key ON map.grid_id = grid_key.grid_id JOIN keymap ON grid_key.key_name = keymap.key_name;");
				}
			}
		}

		public double[] zoomLatLng()
		{
			double[] zoomLatLng = new double[3]{
				13,134.636563726665,34.7894764979235
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
				134.574450763296,34.7497420997416,134.698676690035,34.8292108961054
			};

			return bounds;
		}

		//x,y,zから、画像URLをStringで返す
		public string GetUrl (int x, int y, int z)
		{
			//TMS形式はGoogleのY座標と正負の方向が逆なので直す
			y = (int)Math.Pow (2.0, (double)z) - y - 1;

			string url = String.Format (regexURL, x, y, z);
			return url;
		}

		//x,y,zから、MBTilesに画像イメージがあればバイト列で返す
		public virtual byte[] GetCachedTile (int x, int y, int z)
		{
			byte[] image = null;

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

		//x,y,zで、MBTilesに画像イメージをバイト列で書き込む
		public virtual void SetCachedTile (int x, int y, int z, byte[] image)
		{
			if (image == null) return;

			//書き込みが集中した時のキューに入れるキーとして、x,y,zをセットで保持するための構造体を作る
			var xyz = new XYZOOM { 
				x = x,
				y = y,
				zoom = z
			};

			//ロック
			lock (lockObj) {
				//キューの中に入っていなければ入れる
				if (!cacheTileQueIndex.Contains (xyz)) {
					cacheTileQueIndex.Add (xyz);
					cacheTileQueBag.Add (xyz, image);
				}
				//現在書き込み処理中ならばリターン、未処理ならば書き込み処理に移る
				if (cacheTileWorking) {
					return;
				} else {
					cacheTileWorking = true;
				}
			}

			//バックグラウンド処理のため
			var worker = new BackgroundWorker();

			worker.DoWork += (object sender, DoWorkEventArgs e) => {
				//キューが空になるまで続ける
				while (true) {
					XYZOOM lxyz;
					byte[] limg;
					//ロック
					lock (lockObj) {
						//キューが空ならループ終了
						if (cacheTileQueIndex.Count == 0) break;
						//キューがあれば、キューからx,y,z,imageを取り出し
						lxyz = cacheTileQueIndex[0];
						cacheTileQueIndex.RemoveAt(0);
						limg = cacheTileQueBag[lxyz];
						cacheTileQueBag.Remove(lxyz);
					} 

					//imageの値からMD5ハッシュを生成し、MBTilesに書き込む
					//以下、正規化されたスキーマのために煩雑になっているが、基本的にx,y,z,imageを書き込んでいるだけ
					var md5   = new MD5CryptoServiceProvider();
					var bs    = md5.ComputeHash(limg);
					md5.Clear();

					var result = new StringBuilder();
					foreach (var b in bs) 
					{
						result.Append(b.ToString("x2"));
					}
					var sres = result.ToString();

					using (var conn = new SQLiteConnection(this.dbPath)) {
						var query = conn.Table<images>().Where (v => v.tile_id == sres);

						if (query.Count() == 0) {
							var imQ = conn.Insert(new images() {
								tile_id   = sres,
								tile_data = limg
							});
						}

						var mpQ = conn.Insert(new map() {
							tile_column = lxyz.x,
							tile_row    = lxyz.y,
							zoom_level  = lxyz.zoom,
							tile_id     = sres
						});
					}
				}

				//ロック
				lock(lockObj) {
					//ループを抜ければ書き込み中フラグをオフ
					cacheTileWorking = false;
				}

				worker = null;
			};

			//バックグラウンド実行
			worker.RunWorkerAsync ();
		}
	}
}

