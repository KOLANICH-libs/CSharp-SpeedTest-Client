using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Net.NetworkInformation;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace SpeedTest
{
	
	public class SpeedTest:IDisposable
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
		SpeedTestServer closestKnownServer;

		List<SpeedTestServer> servers;
		WebClient wc;
		System.Diagnostics.Stopwatch sw;

		public delegate void ProgressListener(SpeedTest sender, int progress);
		public delegate void SuccessListener(SpeedTest sender, float speed);
		public event ProgressListener onProgress;
		public event SuccessListener onSuccess;

		public SpeedTest() {
			sw= new System.Diagnostics.Stopwatch();
			wc = new WebClient();
			wc.BaseAddress = speedTestBaseUri.ToString();
			wc.Headers.Add("Accept-Encoding","identity");
			//wc.Headers["Accept-Language"] = "q=0.8,en-us;q=0.5,en;q=0.3";
			//wc.Headers["Cache-Control"] = "max-age=0";
			wc.Headers.Add("DNT","1");

			#if DEBUG
			const string fiddlerProxyAddr = "127.0.0.1:8888";
			if(MessageBox.Show("would you like to use Fiddler proxy ("+fiddlerProxyAddr+")?","use proxy",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
				wc.Proxy = new WebProxy(fiddlerProxyAddr);
			#endif
			//wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressProcesser);
			//wc.UploadProgressChanged += new UploadProgressChangedEventHandler(progressProcesser);
			
		}

		public async Task<int> getConfig(){
			stage = Stage.GETTING_CONFIG;
			wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; rv:20.0) Gecko/20121202 Firefox/20.0");
			wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
			string config = await wc.DownloadStringTaskAsync(speedTestConfigUri);
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

		public float haversine(SpeedTestServer serv){
			//http://www.stormconsultancy.co.uk/blog/development/code-snippets/the-haversine-formula-in-c-and-sql/
			//modified by me a little
			//@returns angular distance
			double dLat = (serv.lat-lat)/180*Math.PI;
			double dLon = (serv.lon-lon)/180*Math.PI;
 
			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(lat/180*Math.PI) *Math.Cos(serv.lon/180*Math.PI) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
			return (float)(2 * Math.Asin(Math.Min(1, Math.Sqrt(a))));
		}

		protected void convertServersXMLToServersList(XmlNodeList servers){
			for (int i = 1; i < servers.Count;i++){
				var server = new SpeedTestServer(servers[i]);
				this.servers.Add(server);
			}
		}

		protected async Task<SpeedTestServer> convertServersXMLToFilteredServersList(XmlNodeList servers){
			if (servers.Count == 0) return null;
			closestKnownServer = new SpeedTestServer(servers[0]);
			closestKnownServer.dist=haversine(closestKnownServer);
			this.servers.Add(closestKnownServer);
			var pingTasks = new List<Task>();

			for (int i = 1; i < servers.Count;i++){
				var server = new SpeedTestServer(servers[i]);
				server.dist = haversine(server);

				if (closestKnownServer.dist - server.dist > distTreshold){
					closestKnownServer = server;
					this.servers.Add(server);
					this.servers.RemoveAt(0);
				}
				else if (Math.Abs(closestKnownServer.dist - server.dist) <= distTreshold){
					this.servers.Add(server);
					//BUG: we need to enable it but it causes hang
					pingTasks.Add(
						Task.Run(async () => {
							await server.ping();
							if (closestKnownServer.latency > server.latency){
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

		public async Task<SpeedTestServer> getServers(bool onlyClosest=true){
			stage = Stage.GETTING_SERVERS;
		
				wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; rv:20.0) Gecko/20121202 Firefox/20.0");
				wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
				string serversXml = await wc.DownloadStringTaskAsync(speedTestServersUri);
				var xml = new XmlDocument();
				XmlNodeList servers = xml.GetElementsByTagName("server");
				xml.LoadXml(serversXml);
				this.servers = new List<SpeedTestServer>();

				stage = Stage.PROCESSING_SERVERS;
				if (onlyClosest){
					return await convertServersXMLToFilteredServersList(servers);
				}
				convertServersXMLToServersList(servers);

				return null;
		}

		public async Task pingServers() {
			foreach (SpeedTestServer server in servers){
				await server.ping();
			}
		}
		public Task<float> measureDownloadSpeed(SpeedTestServer server,int size=100){
			Contract.Requires(server!=null);
			stage = Stage.DOWNLOAD;
			return Task.Factory.StartNew(()=>{	
				var uri = new Uri(server.uri, "random" + "4000x4000" + ".jpg");
				var req = WebRequest.Create(uri);
				req.Credentials = CredentialCache.DefaultCredentials;
				req.Proxy = wc.Proxy;
				req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				emitProgressEvent(0);
				sw.Restart();
				var resp = req.GetResponse() as HttpWebResponse;
				var str = resp.GetResponseStream();


				//for (long i = 0; i < resp.ContentLength;i++) str.ReadByte();
				var array = new byte[resp.ContentLength];
				//str.Flush();
				str.Read(array, 0, (int)resp.ContentLength);
				sw.Stop();
				str.Close();

				var speed = (float)(resp.ContentLength / (sw.Elapsed.TotalSeconds * 1024));
				emitProgressEvent(100);
				emitMeasuredEvent(speed);
				return speed;
			});
		}
		public async Task<float> measureDownloadSpeedAll(int size = 100) {
			double speed=0.0;
			foreach(SpeedTestServer server in servers){
				speed+=await measureDownloadSpeed(server,size);
			}
			return (float)(speed/servers.Count);
		}
		public Task<float> measureDownloadSpeed(int size = 100) {
			return measureDownloadSpeed(closestKnownServer, size);
		}
		public Task<float> measureUploadSpeed(SpeedTestServer server,int size=1024*482){
			Contract.Requires(server != null);
			stage = Stage.UPLOAD;
			return Task.Factory.StartNew(() =>{	
				var req = WebRequest.Create(new Uri(server.uri, "upload.php")) as HttpWebRequest;
				req.Method = "POST";
				req.ContentType = "application/x-www-form-urlencoded";
				req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				//req.ContentLength = size;
				req.ContentLength = 6*(62464 + 10);
				req.KeepAlive = true;

				req.Credentials = CredentialCache.DefaultCredentials;
				req.Proxy = wc.Proxy;
				emitProgressEvent(0);
				var str = req.GetRequestStream();

				/*
				sw.Restart();
				byte[] attribName = Encoding.ASCII.GetBytes("content");
				for (int j = 0; j < 6; j++)
				{
					str.Write(attribName, 0, attribName.Length);
					str.WriteByte((byte) (j + 0x30)); //number
					str.WriteByte((byte) '=');
					for (long i = 0; i < 62464; i++) str.WriteByte((byte) 'A');
					str.WriteByte((byte) '&');
					//emitProgressEvent(100*j/6);
				}*/
				var up = new byte[62464 + 10];
				up[0] = (byte)'c';
				up[1] = (byte)'o';
				up[2] = (byte)'n';
				up[3] = (byte)'t';
				up[4] = (byte)'e';
				up[5] = (byte)'n';
				up[6] = (byte)'t';
				up[8] = (byte)'=';
				up[up.Length-1] = (byte)'&';
				for (long i = 0; i < 62464; i++) up[9+i]=(byte)'A';

				sw.Restart();
				for (int j = 0; j < 6; j++) {
					up[7]=(byte)(j + 0x30); //number
					str.Write(up, 0, 62464 + 10);
				}

				str.Flush();
				sw.Stop();
				str.Close();
				req.GetResponse().GetResponseStream().Close();
				float speed = (float) (req.ContentLength/(sw.Elapsed.TotalSeconds*1024));
				emitProgressEvent(100);
				emitMeasuredEvent(speed);
				return speed;
			});
		}
		public Task<float> measureUploadSpeed(int size = 100) {
			return measureUploadSpeed(closestKnownServer, size);
		}
		public async Task measureSpeed(){
			await measureDownloadSpeed();
			await measureUploadSpeed();
		}

		protected void emitProgressEvent(int progress) {//TODO: fix for the case of averaging (see measureDownloadSpeedAll)
			//onProgress(this, new System.ComponentModel.ProgressChangedEventArgs( ((int)stage*100+progress) / (int)SpeedTestStage.NumberOfTypes, this));
			//onProgress(this, new System.ComponentModel.ProgressChangedEventArgs(progress,this));
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

		protected virtual void Dispose(bool disposing) {
			if (disposing && (wc != null)) {
				wc.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
		}
	}
}
