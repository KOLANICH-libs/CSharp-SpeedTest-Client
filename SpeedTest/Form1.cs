using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedTest
{
	public partial class Form1 : Form
	{
		SpeedTest speedTest;
		public Form1() {
			InitializeComponent();
			speedTest = new SpeedTest();
			speedTest.onProgress += new SpeedTest.ProgressListener(progressProcesser);
			speedTest.onSuccess += new SpeedTest.SuccessListener(successProcesser);
		}
		private void progressProcesser(SpeedTest st, int progress) {
			switch (st.stage)
			{
				case SpeedTest.Stage.UPLOAD:
					uploadProgress.Value=progress;
				break;
				case SpeedTest.Stage.DOWNLOAD:
					downloadProgress.Value = progress;
				break;
			}
		}
		private void successProcesser(SpeedTest st, float speed) {
			switch (st.stage)
			{
				case SpeedTest.Stage.UPLOAD:
					uploadSpeedLbl.Text = "Upload speed: " + speed + " KiBpS";
				break;
				case SpeedTest.Stage.DOWNLOAD:
					downloadSpeedLbl.Text = "Download speed: " + speed + " KiBpS";
				break;
			}
		}

		private void startTestBtn_Click(object sender, EventArgs e) {
			
			speedTest.measureSpeed();
		}

		private void Form1_Load(object sender, EventArgs e) {
			speedTest.getConfig();
			ISPLbl.Text += speedTest.isp;
			custLatLbl.Text += speedTest.lat;
			custLonLbl.Text += speedTest.lon;
			SpeedTestServer nearest=speedTest.getServers();
			pingLbl.Text += nearest.latency;
			serverLbl.Text += nearest.uri.Host;
			servLatLbl.Text += nearest.lat;
			servLonLbl.Text += nearest.lon;
			sponsorLbl.Text += nearest.sponsor;
			servCountryLbl.Text += nearest.country;
		}

	}
}
