using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32;
using MeasuringStreams;

namespace SpeedTest
{
	public class SpeedTest:IDisposable
	{
		public enum Stage : uint
		{
			GETTING_CONFIG,
			GETTING_SERVERS,
			PROCESSING_SERVERS,
			DOWNLOAD,
			UPLOAD,
			NumberOfTypes
		};
		public static readonly Uri speedTestBaseUri=new Uri(@"http://www.speedtest.net/");
		public static readonly Uri speedTestConfigUri = new Uri(speedTestBaseUri, @"speedtest-config.php");
		public static readonly Uri speedTestServersUri = new Uri(speedTestBaseUri, @"speedtest-servers.php");
		public static readonly Uri speedTestAPIUri = new Uri(speedTestBaseUri, @"api/api.php");
		public static readonly Uri speedTestRefererUri = new Uri(@"http://c.speedtest.net/flash/speedtest.swf");
		public static readonly Uri speedTestConfigUrl = new Uri(speedTestBaseUri, @"result/{0}.png");
		const float distTreshold = 0.001f;

		public float lon, lat;
		public string isp;
		public Stage stage;
		Server closestKnownServer;

		List<Server> servers;
		
		HttpClientHandler hch=new HttpClientHandler();
		HttpClient hc;
		
		System.Diagnostics.Stopwatch sw;

		public delegate void ProgressListener(SpeedTest sender, float progress);
		public delegate void SuccessListener(SpeedTest sender, float speed);
		public event ProgressListener onProgress;
		public event SuccessListener onSuccess;

		public SpeedTest(){
			hc = new HttpClient(hch);
			sw = new System.Diagnostics.Stopwatch();
			hc.BaseAddress = speedTestBaseUri;
			hc.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
			hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; rv:27.0) Gecko/20100101 Firefox/27.0");
			hc.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			//wc.Headers["Accept-Language"] = "q=0.8,en-us;q=0.5,en;q=0.3";
			//wc.Headers["Cache-Control"] = "max-age=0";
			hc.DefaultRequestHeaders.Add("DNT", "1");
			hch.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
			hch.UseCookies = false;
			#if DEBUG
			{
				var processes = System.Diagnostics.Process.GetProcessesByName("Fiddler");
				if (processes.Length != 0) {
					try {
						var port = (int) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Fiddler2", "ListenPort", 4444);
						hch.Proxy = new WebProxy("127.0.0.1", port);
					}
					catch (Exception) {}
				}
			}
			#endif
			//wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressProcesser);
			//wc.UploadProgressChanged += new UploadProgressChangedEventHandler(progressProcesser);
			
		}

		public async Task<int> getConfig(){
			stage = Stage.GETTING_CONFIG;
			string config = await hc.GetStringAsync(speedTestConfigUri);
			var xml = new XmlDocument();
			xml.LoadXml(config);
			var client = xml.GetElementsByTagName("client")[0];
			lat = float.Parse(client.Attributes["lat"].Value, System.Globalization.CultureInfo.InvariantCulture);
			lon = float.Parse(client.Attributes["lon"].Value, System.Globalization.CultureInfo.InvariantCulture);
			isp = client.Attributes["isp"].Value;
			/*xml.GetElementsByTagName("times");
			xml.GetElementsByTagName("download");
			xml.GetElementsByTagName("upload");*/
			return 1;
		}

		public float haversine(Server serv){
			//http://www.stormconsultancy.co.uk/blog/development/code-snippets/the-haversine-formula-in-c-and-sql/
			//modified by me a little
			//@returns angular distance
			Contract.Requires(serv != null);
			Contract.Requires(!(float.IsNaN(serv.lat)|| float.IsNegativeInfinity(serv.lat)|| float.IsPositiveInfinity(serv.lat)));
			Contract.Requires(!(float.IsNaN(serv.lon) || float.IsNegativeInfinity(serv.lon) || float.IsPositiveInfinity(serv.lon)));
			double dLat = (serv.lat-lat)/180*Math.PI;
			double dLon = (serv.lon-lon)/180*Math.PI;
 
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(lat/180*Math.PI) *Math.Cos(serv.lon/180*Math.PI) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			return (float)(2 * Math.Asin(Math.Min(1, Math.Sqrt(a))));
		}

		protected void convertServersXMLToServersList(XmlNodeList servers){
			Contract.Requires(servers != null);
			for (int i = 1; i < servers.Count;i++){
				var server = new Server(servers[i]);
				this.servers.Add(server);
			}
		}

		protected async Task<Server> convertServersXMLToFilteredServersList(XmlNodeList servers){
			Contract.Requires(servers != null);
			if (servers.Count == 0) return null;
			closestKnownServer = new Server(servers[0]);
			closestKnownServer.dist=haversine(closestKnownServer);
			this.servers.Add(closestKnownServer);
			var pingTasks = new List<Task>();

			for (int i = 1; i < servers.Count;i++){
				Server server;
				try {
					server = new Server(servers[i]);
				} catch {
					continue;
				}
				server.dist = haversine(server);

				if (closestKnownServer.dist - server.dist > distTreshold) {
					closestKnownServer = server;
					this.servers.Add(server);
					this.servers.RemoveAt(0);
				}
				else if (Math.Abs(closestKnownServer.dist - server.dist) <= distTreshold) {
					this.servers.Add(server);
					//BUG: we need to enable it but it causes hang
					pingTasks.Add(
						Task.Run(async () =>{
							await server.ping();
							if (closestKnownServer.latency > server.latency) {
								closestKnownServer = server;
								this.servers.RemoveAt(0);
							}
						})
					);
				}
			}
			await Task.WhenAll(pingTasks);//bug : it writes it has finished before it is really finished
			return closestKnownServer;
		}

		public async Task<Server> getServers(bool onlyClosest=true){
			stage = Stage.GETTING_SERVERS;
			string serversXml = await hc.GetStringAsync(speedTestServersUri);
			var xml = new XmlDocument();
			XmlNodeList servers = xml.GetElementsByTagName("server");
			xml.LoadXml(serversXml);
			this.servers = new List<Server>();

			stage = Stage.PROCESSING_SERVERS;
			if (onlyClosest){
				return await convertServersXMLToFilteredServersList(servers);
			}
			convertServersXMLToServersList(servers);

			return null;
		}

		public async Task pingServers() {
			foreach (Server server in servers){
				await server.ping();
			}
		}

		public async Task<float> measureDownloadSpeed(Uri uri){
			Contract.Requires(uri != null);
			var req = WebRequest.Create(uri);
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Proxy = hch.Proxy;
			req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			emitProgressEvent(0);
			float speed;
			using (var devNull = new DevNullStream()) {
				using (var resp = await req.GetResponseAsync() as HttpWebResponse) {
					var len = resp.ContentLength;
					EventedStream.dataLoadListener cb = (o,bytes) => emitProgressEvent(bytes*100/len);
					using (var str = resp.GetResponseStream()) {
						devNull.onLoading += cb;
						sw.Restart();
						await str.CopyToAsync(devNull);
						sw.Stop();
						devNull.onLoading -= cb;
					}
					speed = (float)(len / (sw.Elapsed.TotalSeconds));
				}
			}
			return speed;
		}

		public async Task<float> measureDownloadSpeed(Server server,int size=100){
			Contract.Requires(server != null);
			Contract.Requires(size >= 0);
			stage = Stage.DOWNLOAD;
			var uri = new Uri(server.uri, "random" + "4000x4000" + ".jpg");

			var speed=await measureDownloadSpeed(uri);
			emitProgressEvent(100);//uri = null . WHY?????
			emitMeasuredEvent(speed);
			return speed;
		}
		public async Task<float> measureDownloadSpeedAll(int size = 100) {
			Contract.Requires(size >= 0);
			double speed=0.0;
			foreach(Server server in servers){
				speed+=await measureDownloadSpeed(server,size);
			}
			return (float)(speed/servers.Count);
		}
		public Task<float> measureDownloadSpeed(int size = 100) {
			Contract.Requires(size >= 0);
			return measureDownloadSpeed(closestKnownServer, size);
		}

		const long defaultSize = 0xF400;
		const int countOfPieces = 6;

		public async Task<float> measureUploadSpeed(Server server, int size = 1024*482){
			Contract.Requires(server != null);
			Contract.Requires(size >= 0);
			var prefix = new[]{
				(byte) '&',
				(byte) 'c',
				(byte) 'o',
				(byte) 'n',
				(byte) 't',
				(byte) 'e',
				(byte) 'n',
				(byte) 't',
				(byte) '0',
				(byte) '=',
			};
			stage = Stage.UPLOAD;

			var req = HttpWebRequest.Create(new Uri(server.uri, "upload.php")) as HttpWebRequest;
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			req.ContentLength = countOfPieces*(defaultSize + prefix.Length);
			req.KeepAlive = true;

			req.Credentials = CredentialCache.DefaultCredentials;
			req.Proxy = hch.Proxy;

			emitProgressEvent(0);
			using (var str = req.GetRequestStream()) {
				using (var ums = new RangedRandomBytesStream(defaultSize, (byte)'A', (byte)'z'))
				{
					int j = 0;
                    EventedStream.dataLoadListener evtListener = (o, bytes) => emitProgressEvent(100 * (j + bytes / defaultSize) / countOfPieces);
                    ums.onLoading += evtListener;
                    sw.Restart();
					for (; j < countOfPieces; j++)
					{
						prefix[8] = (byte)('0' + j);
						await str.WriteAsync(prefix, 0, prefix.Length);
						await str.FlushAsync();
						await ums.CopyToAsync(str);
                        ums.Position = 0;
						await str.FlushAsync();
					}
					sw.Stop();
                    ums.onLoading -= evtListener;
                }
			}

			await req.GetResponseAsync().ContinueWith(t => t.Result.GetResponseStream().Close());
			float speed = (float) (req.ContentLength/(sw.Elapsed.TotalSeconds));
			emitProgressEvent(100);
			emitMeasuredEvent(speed);
			return speed;
		}
		public Task<float> measureUploadSpeed(int size = 100) {
			return measureUploadSpeed(closestKnownServer, size);
		}
		public async Task measureSpeed(){
			await measureDownloadSpeed();
			await measureUploadSpeed();
		}

		protected void emitProgressEvent(float progress) {//TODO: fix for the case of averaging (see measureDownloadSpeedAll)
			if (onProgress != null)
			{
				onProgress(this, progress);
			}
		}
		protected void emitMeasuredEvent(float speed) {//TODO: fix for the case of averaging (see measureDownloadSpeedAll)
			if (onSuccess != null)
			{
				onSuccess(this, speed);
			}
		}

		/*private void progressProcesser(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
			emitProgressEvent(e.ProgressPercentage);
		}*/

		public static string speedToHumanReadable(float speed, short bs=1024, string mod="i", string baseUnit="b/s"){
			byte order = (byte) Math.Floor(Math.Log(speed, bs));
			string unit;
			switch (order) {
				default :
					unit = "";
				break;
				case 1:
					unit = "K";
				break;
				case 2:
					unit = "M";
				break;
				case 3:
					unit = "G";
				break;
				case 4:
					unit = "T";
				break;
				case 5:
					unit = "P";
				break;
			}
			if (unit != "") {
				unit = unit + mod;
				speed /= (float)Math.Pow(bs, order);
			}
			return String.Format("{0} {1}{2}", speed, unit, baseUnit);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if(hc != null) hc.Dispose();
				if(hch != null) hch.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
		}
	}
}
