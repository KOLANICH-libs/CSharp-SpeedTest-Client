namespace SpeedTest
{
	partial class Form1
	{
		/// <summary>
		/// Требуется переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Обязательный метод для поддержки конструктора - не изменяйте
		/// содержимое данного метода при помощи редактора кода.
		/// </summary>
		private void InitializeComponent() {
			this.downloadProgress = new System.Windows.Forms.ProgressBar();
			this.uploadProgress = new System.Windows.Forms.ProgressBar();
			this.ISPLbl = new System.Windows.Forms.Label();
			this.custLatLbl = new System.Windows.Forms.Label();
			this.custLonLbl = new System.Windows.Forms.Label();
			this.uploadSpeedLbl = new System.Windows.Forms.Label();
			this.downloadSpeedLbl = new System.Windows.Forms.Label();
			this.startTestBtn = new System.Windows.Forms.Button();
			this.pingLbl = new System.Windows.Forms.Label();
			this.serverLbl = new System.Windows.Forms.Label();
			this.servLonLbl = new System.Windows.Forms.Label();
			this.servLatLbl = new System.Windows.Forms.Label();
			this.sponsorLbl = new System.Windows.Forms.Label();
			this.servCountryLbl = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// downloadProgress
			// 
			this.downloadProgress.Location = new System.Drawing.Point(12, 230);
			this.downloadProgress.Name = "downloadProgress";
			this.downloadProgress.Size = new System.Drawing.Size(602, 48);
			this.downloadProgress.TabIndex = 0;
			// 
			// uploadProgress
			// 
			this.uploadProgress.Location = new System.Drawing.Point(12, 301);
			this.uploadProgress.Name = "uploadProgress";
			this.uploadProgress.Size = new System.Drawing.Size(602, 53);
			this.uploadProgress.TabIndex = 1;
			// 
			// ISPLbl
			// 
			this.ISPLbl.AutoSize = true;
			this.ISPLbl.Location = new System.Drawing.Point(36, 27);
			this.ISPLbl.Name = "ISPLbl";
			this.ISPLbl.Size = new System.Drawing.Size(30, 13);
			this.ISPLbl.TabIndex = 2;
			this.ISPLbl.Text = "ISP: ";
			// 
			// custLatLbl
			// 
			this.custLatLbl.AutoSize = true;
			this.custLatLbl.Location = new System.Drawing.Point(225, 27);
			this.custLatLbl.Name = "custLatLbl";
			this.custLatLbl.Size = new System.Drawing.Size(51, 13);
			this.custLatLbl.TabIndex = 3;
			this.custLatLbl.Text = "Latitude: ";
			// 
			// custLonLbl
			// 
			this.custLonLbl.AutoSize = true;
			this.custLonLbl.Location = new System.Drawing.Point(365, 27);
			this.custLonLbl.Name = "custLonLbl";
			this.custLonLbl.Size = new System.Drawing.Size(60, 13);
			this.custLonLbl.TabIndex = 4;
			this.custLonLbl.Text = "Longitude: ";
			// 
			// uploadSpeedLbl
			// 
			this.uploadSpeedLbl.AutoSize = true;
			this.uploadSpeedLbl.Location = new System.Drawing.Point(36, 197);
			this.uploadSpeedLbl.Name = "uploadSpeedLbl";
			this.uploadSpeedLbl.Size = new System.Drawing.Size(81, 13);
			this.uploadSpeedLbl.TabIndex = 5;
			this.uploadSpeedLbl.Text = "Upload Speed: ";
			// 
			// downloadSpeedLbl
			// 
			this.downloadSpeedLbl.AutoSize = true;
			this.downloadSpeedLbl.Location = new System.Drawing.Point(356, 197);
			this.downloadSpeedLbl.Name = "downloadSpeedLbl";
			this.downloadSpeedLbl.Size = new System.Drawing.Size(95, 13);
			this.downloadSpeedLbl.TabIndex = 6;
			this.downloadSpeedLbl.Text = "Download Speed: ";
			// 
			// startTestBtn
			// 
			this.startTestBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.startTestBtn.Location = new System.Drawing.Point(501, 27);
			this.startTestBtn.Name = "startTestBtn";
			this.startTestBtn.Size = new System.Drawing.Size(113, 103);
			this.startTestBtn.TabIndex = 7;
			this.startTestBtn.Text = "START\r\nTEH\r\nTEST";
			this.startTestBtn.UseVisualStyleBackColor = true;
			this.startTestBtn.Click += new System.EventHandler(this.startTestBtn_Click);
			// 
			// pingLbl
			// 
			this.pingLbl.AutoSize = true;
			this.pingLbl.Location = new System.Drawing.Point(365, 152);
			this.pingLbl.Name = "pingLbl";
			this.pingLbl.Size = new System.Drawing.Size(34, 13);
			this.pingLbl.TabIndex = 8;
			this.pingLbl.Text = "Ping: ";
			// 
			// serverLbl
			// 
			this.serverLbl.AutoSize = true;
			this.serverLbl.Location = new System.Drawing.Point(36, 89);
			this.serverLbl.Name = "serverLbl";
			this.serverLbl.Size = new System.Drawing.Size(44, 13);
			this.serverLbl.TabIndex = 9;
			this.serverLbl.Text = "Server: ";
			// 
			// servLonLbl
			// 
			this.servLonLbl.AutoSize = true;
			this.servLonLbl.Location = new System.Drawing.Point(365, 89);
			this.servLonLbl.Name = "servLonLbl";
			this.servLonLbl.Size = new System.Drawing.Size(60, 13);
			this.servLonLbl.TabIndex = 4;
			this.servLonLbl.Text = "Longitude: ";
			// 
			// servLatLbl
			// 
			this.servLatLbl.AutoSize = true;
			this.servLatLbl.Location = new System.Drawing.Point(225, 89);
			this.servLatLbl.Name = "servLatLbl";
			this.servLatLbl.Size = new System.Drawing.Size(51, 13);
			this.servLatLbl.TabIndex = 3;
			this.servLatLbl.Text = "Latitude: ";
			// 
			// sponsorLbl
			// 
			this.sponsorLbl.AutoSize = true;
			this.sponsorLbl.Location = new System.Drawing.Point(36, 152);
			this.sponsorLbl.Name = "sponsorLbl";
			this.sponsorLbl.Size = new System.Drawing.Size(52, 13);
			this.sponsorLbl.TabIndex = 10;
			this.sponsorLbl.Text = "Sponsor: ";
			// 
			// servCountryLbl
			// 
			this.servCountryLbl.AutoSize = true;
			this.servCountryLbl.Location = new System.Drawing.Point(197, 152);
			this.servCountryLbl.Name = "servCountryLbl";
			this.servCountryLbl.Size = new System.Drawing.Size(49, 13);
			this.servCountryLbl.TabIndex = 11;
			this.servCountryLbl.Text = "Country: ";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(628, 366);
			this.Controls.Add(this.servCountryLbl);
			this.Controls.Add(this.sponsorLbl);
			this.Controls.Add(this.serverLbl);
			this.Controls.Add(this.pingLbl);
			this.Controls.Add(this.startTestBtn);
			this.Controls.Add(this.downloadSpeedLbl);
			this.Controls.Add(this.uploadSpeedLbl);
			this.Controls.Add(this.servLonLbl);
			this.Controls.Add(this.custLonLbl);
			this.Controls.Add(this.servLatLbl);
			this.Controls.Add(this.custLatLbl);
			this.Controls.Add(this.ISPLbl);
			this.Controls.Add(this.uploadProgress);
			this.Controls.Add(this.downloadProgress);
			this.Name = "Form1";
			this.Text = "SpeedTest";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar downloadProgress;
		private System.Windows.Forms.ProgressBar uploadProgress;
		private System.Windows.Forms.Label ISPLbl;
		private System.Windows.Forms.Label custLatLbl;
		private System.Windows.Forms.Label custLonLbl;
		private System.Windows.Forms.Label uploadSpeedLbl;
		private System.Windows.Forms.Label downloadSpeedLbl;
		private System.Windows.Forms.Button startTestBtn;
		private System.Windows.Forms.Label pingLbl;
		private System.Windows.Forms.Label serverLbl;
		private System.Windows.Forms.Label servLonLbl;
		private System.Windows.Forms.Label servLatLbl;
		private System.Windows.Forms.Label sponsorLbl;
		private System.Windows.Forms.Label servCountryLbl;
	}
}

