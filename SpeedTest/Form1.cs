using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpeedTest.Properties;

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
			if (InvokeRequired)
				BeginInvoke((Action<SpeedTest, int>)progressProcesser, st, progress);
			else {
				switch (st.stage) {
					case SpeedTest.Stage.UPLOAD:
						uploadProgress.Value = progress;
					break;
					case SpeedTest.Stage.DOWNLOAD:
						downloadProgress.Value = progress;
					break;
				}
			}
		}
		private void successProcesser(SpeedTest st, float speed){
			if (InvokeRequired)
				BeginInvoke((Action<SpeedTest, float>)successProcesser, st, speed);
			else {
				switch (st.stage) {
					case SpeedTest.Stage.UPLOAD:
						startTestBtn.Enabled = true;
						uploadSpeedLbl.Text = Resources.Upload_speed + speed/1024 + " MiBpS";
					break;
					case SpeedTest.Stage.DOWNLOAD:
						downloadSpeedLbl.Text = Resources.Download_speed + speed/1024 + " MiBpS";
					break;
				}
			}
		}

		private async void startTestBtn_Click(object sender, EventArgs e) {
			startTestBtn.Enabled = false;
			await speedTest.measureSpeed();
		}

		private async void Form1_Load(object sender, EventArgs e){
			startTestBtn.Enabled = false;
			await speedTest.getConfig();
			ISPLbl.Text += speedTest.isp;
			custLatLbl.Text += speedTest.lat;
			custLonLbl.Text += speedTest.lon;
			SpeedTestServer nearest=await speedTest.getServers();
			pingLbl.Text += nearest.latency;
			serverLbl.Text += nearest.uri.Host;
			servLatLbl.Text += nearest.lat;
			servLonLbl.Text += nearest.lon;
			sponsorLbl.Text += nearest.sponsor;
			servCountryLbl.Text += nearest.country;
			startTestBtn.Enabled = true;
		}

	}
}
