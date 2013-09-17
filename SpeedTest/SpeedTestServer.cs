using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SpeedTest
{
	public class SpeedTestServer : IComparable
	{
		public Uri uri;
		public int id;
		public float lat, lon, dist;
		public string name, sponsor;
		public long _latency;
		public long latency {
			get {
				if (_latency == -1) {
					var t=ping();
					//t.Wait();//BUG: we need to enable it but it causes hang
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
				) { }
		public SpeedTestServer(int id, string name, string uri, float lat, float lon, string country, string sponsor)
			: this(
				id,
				name,
				new Uri(uri.Substring(0, uri.LastIndexOf('/') + 1)),
				lat,
				lon,
				//new System.Globalization.RegionInfo(country),
				country,
				sponsor
				) { }
		public async Task<long> ping(){
			_latency = long.MaxValue;
			var pingSender = new Ping();
			var reply = await pingSender.SendPingAsync(uri.Host,54);
			_latency = reply.Status == IPStatus.Success ? reply.RoundtripTime : long.MaxValue;
			return _latency;
		}

		public int CompareTo(object obj) {
			var serv = obj as SpeedTestServer;
			return latency.CompareTo(serv.latency);
		}
	}
}
