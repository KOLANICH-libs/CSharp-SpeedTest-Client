using System;
using System.Windows;
using System.Windows.Shell;

namespace SpeedTestGui
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IDisposable
	{
		private SpeedTest.SpeedTest test=new SpeedTest.SpeedTest();
		private SpeedTest.SpeedTest.ProgressListener bytesLoadListener;
		public MainWindow() {
			InitializeComponent();
			bytesLoadListener = progressListener;
			test.onProgress +=bytesLoadListener;
			TaskbarItemInfo = new TaskbarItemInfo();
		}

		private async void Button_Click(object sender, RoutedEventArgs e){
			startBtn.IsEnabled = false;
			infoBlk.Text = null;
			TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
			TaskbarItemInfo.ProgressValue = 0;
			var speed=await test.measureDownloadSpeed(new Uri(urlBox.Text));
			infoBlk.Text = SpeedTest.SpeedTest.speedToHumanReadable(speed);
			TaskbarItemInfo.Description = infoBlk.Text;
			startBtn.IsEnabled = true;
		}

		private void progressListener(SpeedTest.SpeedTest sender, float progress) {
			if (Dispatcher.CheckAccess()) {
				downBar.Value = progress;
				infoBlk.Text = progress + " %";
				TaskbarItemInfo.ProgressValue = progress/100;
				TaskbarItemInfo.Description = infoBlk.Text;
			}
			else
				Dispatcher.Invoke(bytesLoadListener, new object[]{sender, progress});
		}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    test.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~MainWindow() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

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
