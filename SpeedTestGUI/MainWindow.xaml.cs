using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace SpeedTestGUIWPF
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow :IDisposable
	{
		readonly SpeedTest.SpeedTest speedTest= new SpeedTest.SpeedTest();
		private SpeedTest.SpeedTest.ProgressListener progressListener;
		private SpeedTest.SpeedTest.SuccessListener successListener;

		public MainWindow() {
			InitializeComponent();
			progressListener = progressProcesser;
			speedTest.onProgress += progressListener;
			successListener = successProcesser;
			speedTest.onSuccess += successListener;
			TaskbarItemInfo=new TaskbarItemInfo();
		}

		private void progressProcesser(SpeedTest.SpeedTest sender, float progress) {
			Contract.Requires(sender != null);
			Contract.Requires(!float.IsNaN(progress));
			if (Dispatcher.CheckAccess()) {
				switch (sender.stage) {
					case SpeedTest.SpeedTest.Stage.UPLOAD:
						uploadProgress.Value = progress;
					break;
					case SpeedTest.SpeedTest.Stage.DOWNLOAD:
						downloadProgress.Value = progress;
					break;
				}
				TaskbarItemInfo.ProgressValue = (uploadProgress.Value + downloadProgress.Value) / 2;
				TaskbarItemInfo.Description = (uploadProgress.Value + downloadProgress.Value) / 2 + " %";
			} else
				Dispatcher.Invoke(progressListener, new object[] { sender, progress });
		}

		private void successProcesser(SpeedTest.SpeedTest sender, float speed){
			Contract.Requires(sender != null);
			Contract.Requires(!float.IsNaN(speed));
			//var hRSpeed = SpeedTest.SpeedTest.speedToHumanReadable(speed, 1000, "");
			var hRSpeed = SpeedTest.SpeedTest.speedToHumanReadable(speed);
			if (Dispatcher.CheckAccess()) {
				switch (sender.stage) {
					case SpeedTest.SpeedTest.Stage.DOWNLOAD:
						downloadSpeedLbl.Content = hRSpeed;
                    break;
					case SpeedTest.SpeedTest.Stage.UPLOAD:
						startTestBtn.IsEnabled = true;
						uploadSpeedLbl.Content = hRSpeed;
                        TaskbarItemInfo.Description = uploadSpeedLbl.Content + Environment.NewLine + downloadSpeedLbl.Content;
						TaskbarItemInfo.ProgressState=TaskbarItemProgressState.None;
					break;
					
				}
			} else
				Dispatcher.Invoke(successListener, new object[] { sender, speed });
		}

		private async void startTestBtn_Click(object sender, EventArgs e) {
			startTestBtn.IsEnabled = false;
			TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
			TaskbarItemInfo.ProgressValue = 0;
			await speedTest.measureSpeed();
		}

		private async void Form1_OnLoaded(object sender, RoutedEventArgs e){
			try {
				startTestBtn.IsEnabled = false;
				TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
				await speedTest.getConfig();
				ISPLbl.Content = speedTest.isp;
				custLatLbl.Content = speedTest.lat;
				custLonLbl.Content = speedTest.lon;
				SpeedTest.Server nearest = await speedTest.getServers();
				pingLbl.Content = nearest.latency;
				serverLbl.Content = nearest.uri.Host;
				servLatLbl.Content = nearest.lat;
				servLonLbl.Content = nearest.lon;
				sponsorLbl.Content = nearest.sponsor;
				servCountryLbl.Content = nearest.country;
				startTestBtn.IsEnabled = true;
			} catch (Exception ex) {
				startTestBtn.IsEnabled = false;
				startTestBtn.Content = ex.Message;
				TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
				startTestBtn.Background = new RadialGradientBrush(Color.FromArgb(0xFF,0xFF,0,0),Color.FromArgb(0xFF,0xFF,0x00,0xFF));
			}
		}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    speedTest.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
