using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Net.NetworkInformation;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace SpeedTest
{
	class SpeedTestServer : IComparable
	{
		public Uri uri;
		public int id;
		public float lat, lon, dist;
		public string name, sponsor;
		public long _latency;
		public long latency {
			get {
				if (_latency == -1)
				{
					ping();
				}
				return _latency;
			}
		}

		//System.Globalization.RegionInfo country;
		public string country;//speedtest has wrong codes for countries

		/*public SpeedTestServer(int id, string name, Uri uri, float lat, float lon, System.Globalization.RegionInfo country, string sponsor) {
			this.id = id;
			this.name = name;
			this.uri = uri;
			this.lat = lat;
			this.lon = lon;
			this.country = country;
			this.sponsor = sponsor;
			_latency=-1;
		}*/
		public SpeedTestServer(int id, string name, Uri uri, float lat, float lon, string country, string sponsor) {
			this.id = id;
			this.name = name;
			this.uri = uri;
			this.lat = lat;
			this.lon = lon;
			this.country = country;
			this.sponsor = sponsor;
			_latency = -1;
		}

		public SpeedTestServer(XmlNode xml)
			: this(
				int.Parse(xml.Attributes["id"].Value),
				xml.Attributes["name"].Value,
				xml.Attributes["url"].Value,
				float.Parse(xml.Attributes["lat"].Value, System.Globalization.CultureInfo.InvariantCulture),
				float.Parse(xml.Attributes["lon"].Value, System.Globalization.CultureInfo.InvariantCulture),
				xml.Attributes["cc"].Value,
				xml.Attributes["sponsor"].Value
				) {}
		public SpeedTestServer(int id, string name, string uri, float lat, float lon, string country, string sponsor)
			: this(
				id,
				name,
				new Uri(uri.Substring(0, uri.LastIndexOf('/') + 1 )),
				lat,
				lon,
				//new System.Globalization.RegionInfo(country),
				country,
				sponsor
				) {}
		public long ping() {
			Ping pingSender = new Ping();
			PingReply reply = pingSender.Send(this.uri.Host);
			if (reply.Status == IPStatus.Success)
			{
				_latency=reply.RoundtripTime;
			}
			else
			{
				_latency = long.MaxValue;
			}
			return latency;
		}

		public int CompareTo(object obj) {
			SpeedTestServer serv = obj as SpeedTestServer;
			return latency.CompareTo(serv.latency);
		}
	}

	
	class SpeedTest
	{
		public enum Stage : int
		{
			GETTING_CONFIG,
			GETTING_SERVERS,
			PROCESSING_SERVERS,
			DOWNLOAD,
			UPLOAD,
			NumberOfTypes
		};
		static readonly Uri speedTestBaseUri=new Uri(@"http://www.speedtest.net/");
		static readonly Uri speedTestConfigUri = new Uri(speedTestBaseUri, @"speedtest-config.php");
		static readonly Uri speedTestServersUri = new Uri(speedTestBaseUri, @"speedtest-servers.php");
		static readonly Uri speedTestAPIUri = new Uri(speedTestBaseUri, @"api/api.php");
		static readonly Uri speedTestRefererUri = new Uri(@"http://c.speedtest.net/flash/speedtest.swf");
		static readonly Uri speedTestConfigUrl = new Uri(speedTestBaseUri, @"result/{0}.png");
		const float distTreshold = 0.001f;

		public float lon, lat;
		public string isp;

		List<SpeedTestServer> servers;
		WebClient wc;
		public Stage stage;

		public delegate void ProgressListener(SpeedTest sender, int progress);
		public delegate void SuccessListener(SpeedTest sender, float speed);
		public event ProgressListener onProgress;
		public event SuccessListener onSuccess;

		public SpeedTest() {
			wc = new WebClient();
			wc.BaseAddress = speedTestBaseUri.ToString();
			wc.Headers.Add("Accept-Encoding","identity");
			//wc.Headers["Accept-Language"] = "q=0.8,en-us;q=0.5,en;q=0.3";
			//wc.Headers["Cache-Control"] = "max-age=0";
			wc.Headers.Add("DNT","1");


			//wc.Proxy = new WebProxy("127.0.0.1:8888");
			wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressProcesser);
			wc.UploadProgressChanged += new UploadProgressChangedEventHandler(progressProcesser);
			
		}
		public int getConfig() {
			stage = Stage.GETTING_CONFIG;
			wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; rv:20.0) Gecko/20121202 Firefox/20.0");
			wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			string config=wc.DownloadString(speedTestConfigUri);
			XmlDocument xml=new XmlDocument();
			xml.LoadXml(config);
			var client=xml.GetElementsByTagName("client")[0];
			lat = float.Parse(client.Attributes["lat"].Value, System.Globalization.CultureInfo.InvariantCulture);
			lon = float.Parse(client.Attributes["lon"].Value, System.Globalization.CultureInfo.InvariantCulture);
			isp = client.Attributes["isp"].Value;
			/*xml.GetElementsByTagName("times");
			xml.GetElementsByTagName("download");
			xml.GetElementsByTagName("upload");*/
			return 1;
		}
		public float haversine(SpeedTestServer serv){
			//http://www.stormconsultancy.co.uk/blog/development/code-snippets/the-haversine-formula-in-c-and-sql/
			//modified by me a little
			//@returns angular distance
			double dLat = (serv.lat-lat)/180*Math.PI;
			double dLon = (serv.lon-lon)/180*Math.PI;
 
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(lat/180*Math.PI) *Math.Cos(serv.lon/180*Math.PI) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			float c = (float)(2 * Math.Asin(Math.Min(1, Math.Sqrt(a))));
			return c;
		}

		protected void convertServersXMLToServersList(XmlNodeList servers){
			for (int i = 1; i < servers.Count;i++){
				var server = new SpeedTestServer(servers[i]);
				this.servers.Add(server);
			}
		}

		protected SpeedTestServer convertServersXMLToFilteredServersList(XmlNodeList servers){
			SpeedTestServer closestKnownServer;
			if (servers.Count == 0) return null;
			closestKnownServer = new SpeedTestServer(servers[0]);
			closestKnownServer.dist=haversine(closestKnownServer);
			this.servers.Add(closestKnownServer);

			for (int i = 1; i < servers.Count;i++){
				var server = new SpeedTestServer(servers[i]);
				server.dist=haversine(server);
				if (closestKnownServer.dist - server.dist > distTreshold)
				{
					closestKnownServer = server;
					this.servers.Add(server);
					this.servers.RemoveAt(0);
				}
				else if (Math.Abs(closestKnownServer.dist - server.dist) <= distTreshold)
				{
					this.servers.Add(server);
					if (closestKnownServer.latency > server.latency) {
						closestKnownServer = server;
						this.servers.RemoveAt(0);
					}
				}

			}
			return closestKnownServer;
		}

		public SpeedTestServer getServers(bool onlyClosest=true){
			stage = Stage.GETTING_SERVERS;
			wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; rv:20.0) Gecko/20121202 Firefox/20.0");
			wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			string serversXml=wc.DownloadString(speedTestServersUri);
			XmlDocument xml=new XmlDocument();
			XmlNodeList servers = xml.GetElementsByTagName("server");
			xml.LoadXml(serversXml);
			this.servers = new List<SpeedTestServer>();

			stage = Stage.PROCESSING_SERVERS;
			if (onlyClosest){
				return this.convertServersXMLToFilteredServersList(servers);
			}
			this.convertServersXMLToServersList(servers);

			
			return null;
		}

		public void pingServers() {
			foreach (SpeedTestServer server in servers){
				server.ping();
			}
		}
		public float measureDownloadSpeed(SpeedTestServer server,int size=100) {
			stage = Stage.DOWNLOAD;
			var uri = new Uri(server.uri, "random" + "4000x4000" + ".jpg");
			WebRequest req=WebRequest.Create(uri);
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Proxy = wc.Proxy;
			emitProgressEvent(0);
			var resp=req.GetResponse();
			var str=resp.GetResponseStream();
			long t = DateTime.Now.Ticks;
			for (long i = 0; i < resp.ContentLength;i++) str.ReadByte();
			
			str.Close();
			t = DateTime.Now.Ticks - t;
			float speed = (float)resp.ContentLength * 10000000 / (t * 1024);
			emitProgressEvent(100);
			emitMeasuredEvent(speed);
			return speed;
		}
		public float measureDownloadSpeedAll(int size=100){
			double speed=0.0;
			foreach(SpeedTestServer server in servers){
				speed+=measureDownloadSpeed(server,size);
			};
			return (float)(speed/servers.Count);
		}
		public float measureDownloadSpeed(int size = 100) {
			return measureDownloadSpeed(servers[servers.Count-1], size);
		}
		public float measureUploadSpeed(SpeedTestServer server,int size=1024*482) {
			stage = Stage.UPLOAD;
			HttpWebRequest req = WebRequest.Create(new Uri(server.uri, "upload.php")) as HttpWebRequest;
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			//req.ContentLength = size;
			req.ContentLength = 6 * (62464+10);
			req.KeepAlive=true;

			req.Credentials = CredentialCache.DefaultCredentials;
			req.Proxy = wc.Proxy;
			emitProgressEvent(0);
			long t = DateTime.Now.Ticks;
			var str = req.GetRequestStream();
			//str.SetLength(size);
			byte[] attribName=ASCIIEncoding.ASCII.GetBytes("content");
			for (int j = 0; j < 6; j++){
				str.Write(attribName, 0, attribName.Length);
				str.WriteByte((byte)(j+0x30));//number
				str.WriteByte((byte)'=');
				for (long i = 0; i < 62464; i++) str.WriteByte((byte)'A');
				str.WriteByte((byte)'&');
				emitProgressEvent(100*j/6);
			}
			
			str.Close();
			t = DateTime.Now.Ticks - t;
			req.GetResponse().GetResponseStream().Close();
			
			float speed = (float)req.ContentLength * 10000000 / (t * 1024);
			emitProgressEvent(100);
			emitMeasuredEvent(speed);
			return speed;
		}
		public float measureUploadSpeedSpeed(int size = 100) {
			return measureUploadSpeed(servers[servers.Count - 1], size);
		}
		public void measureSpeed() {
			Thread measuringThread = new Thread(delegate() {
				measureDownloadSpeed();
				measureUploadSpeedSpeed();
			});
			measuringThread.Start();
		}

		protected void emitProgressEvent(int progress) {//TODO: fix for the case of averaging (see measureDownloadSpeedAll)
			//onProgress(this, new System.ComponentModel.ProgressChangedEventArgs( ((int)stage*100+progress) / (int)SpeedTestStage.NumberOfTypes, this));
			//onProgress(this, new System.ComponentModel.ProgressChangedEventArgs(progress,this));
			onProgress(this, progress);
		}
		protected void emitMeasuredEvent(float speed) {//TODO: fix for the case of averaging (see measureDownloadSpeedAll)
			onSuccess(this, speed);
		}

		private void progressProcesser(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
			emitProgressEvent(e.ProgressPercentage);
		}
	}
}
