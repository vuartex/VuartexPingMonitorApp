using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;


namespace VuartexPingMonitorApp
{


    public partial class Form1 : Form
    {


        // Alanlar
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.ComboBox cmbInterval;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblPingResult;
        private System.Windows.Forms.Button btnSpeedTest;
        private System.Windows.Forms.Label lblDownload;
        private System.Windows.Forms.Label lblUpload;
        private System.Windows.Forms.Label lblJitter;
        private System.Windows.Forms.Button btnSaveUrl;
        private System.Timers.Timer pingTimer;
        private string logFilePath;
        private string downloadsFolder;
        private ToolTip toolTip = new ToolTip();
        private System.Windows.Forms.Button btnOpenUrlManager;


        private void btnManageUrls_Click(object sender, EventArgs e)
        {
            var managerForm = new UrlManagerForm();
            managerForm.UrlSelected += (selectedUrl) =>
            {
                txtAddress.Text = selectedUrl;
            };
            managerForm.ShowDialog();
        }

        private void LoadUrlSuggestions()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "saved_urls.txt");
            if (File.Exists(path))
            {
                var urls = File.ReadAllLines(path);
                var autoSource = new AutoCompleteStringCollection();
                autoSource.AddRange(urls);
                txtAddress.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtAddress.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtAddress.AutoCompleteCustomSource = autoSource;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            EnsureSpeedtestLicenseAccepted();
            LoadUrlSuggestions();
            cooldownTimer = new System.Windows.Forms.Timer();
            cooldownTimer.Interval = 1000; // 1 saniye
            cooldownTimer.Tick += CooldownTimer_Tick;


            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            // Kullanıcı profili (C:\Users\<Kullanıcı>)
            // Örnek: "C:\Users\<kullanıcı>\Downloads"
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // Downloads klasörü
            downloadsFolder = Path.Combine(userProfile, "Downloads");
            //string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            //MessageBox.Show("Gömülü kaynaklar:\n" + string.Join("\n", names));

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            cooldownCounter--;
            if (cooldownCounter <= 0)
            {
                cooldownTimer.Stop();
                btnStart.Text = "Ping Testini Başlat";
                btnStart.Enabled = true;
            }
            else
            {
                btnStart.Text = $"({cooldownCounter})";
            }
        }


        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.cmbInterval = new System.Windows.Forms.ComboBox();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lblPingResult = new System.Windows.Forms.Label();
            this.btnSpeedTest = new System.Windows.Forms.Button();
            this.lblDownload = new System.Windows.Forms.Label();
            this.lblUpload = new System.Windows.Forms.Label();
            this.lblJitter = new System.Windows.Forms.Label();
            this.btnSaveUrl = new System.Windows.Forms.Button();
            this.linkLabelGitHub = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnOpenUrlManager = new System.Windows.Forms.Button();
            this.btnOpenUrlManager.Location = new System.Drawing.Point(700, 20);
            this.btnOpenUrlManager.Text = "URL Listesi";
            this.btnOpenUrlManager.Click += new System.EventHandler(this.btnOpenUrlManager_Click);
            this.Controls.Add(this.btnOpenUrlManager);
            this.SuspendLayout();
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(20, 20);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(300, 20);
            this.txtAddress.TabIndex = 0;
            this.txtAddress.TextChanged += new System.EventHandler(this.txtAddress_TextChanged);
            // 
            // cmbInterval
            // 
            this.cmbInterval.Items.AddRange(new object[] {
            "15s",
            "30s",
            "2dk",
            "5dk",
            "15dk",
            "30dk",
            "1saat"});
            this.cmbInterval.Location = new System.Drawing.Point(437, 20);
            this.cmbInterval.Name = "cmbInterval";
            this.cmbInterval.Size = new System.Drawing.Size(116, 21);
            this.cmbInterval.TabIndex = 1;
            this.cmbInterval.SelectedIndexChanged += new System.EventHandler(this.cmbInterval_SelectedIndexChanged);
            // 
            // chkLog
            // 
            this.chkLog.Location = new System.Drawing.Point(580, 20);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(150, 22);
            this.chkLog.TabIndex = 2;
            this.chkLog.Text = "Log dosyasına yaz";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(20, 60);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(113, 30);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Ping Testini Başlat";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(139, 60);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(110, 30);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Ping Testini Durdur";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(20, 100);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(760, 157);
            this.txtOutput.TabIndex = 5;
            // 
            // lblPingResult
            // 
            this.lblPingResult.Location = new System.Drawing.Point(20, 267);
            this.lblPingResult.Name = "lblPingResult";
            this.lblPingResult.Size = new System.Drawing.Size(400, 30);
            this.lblPingResult.TabIndex = 6;
            this.lblPingResult.Text = "Son Ping Değeri : ";
            // 
            // btnSpeedTest
            // 
            this.btnSpeedTest.Location = new System.Drawing.Point(20, 300);
            this.btnSpeedTest.Name = "btnSpeedTest";
            this.btnSpeedTest.Size = new System.Drawing.Size(150, 30);
            this.btnSpeedTest.TabIndex = 7;
            this.btnSpeedTest.Text = "İnternet Hız Testi";
            this.btnSpeedTest.UseVisualStyleBackColor = true;
            this.btnSpeedTest.Click += new System.EventHandler(this.btnSpeedTest_Click);
            // 
            // lblDownload
            // 
            this.lblDownload.Location = new System.Drawing.Point(20, 340);
            this.lblDownload.Name = "lblDownload";
            this.lblDownload.Size = new System.Drawing.Size(368, 20);
            this.lblDownload.TabIndex = 8;
            this.lblDownload.Text = "Download: -";
            // 
            // lblUpload
            // 
            this.lblUpload.Location = new System.Drawing.Point(20, 370);
            this.lblUpload.Name = "lblUpload";
            this.lblUpload.Size = new System.Drawing.Size(368, 20);
            this.lblUpload.TabIndex = 9;
            this.lblUpload.Text = "Upload: -";
            // 
            // lblJitter
            // 
            this.lblJitter.Location = new System.Drawing.Point(20, 400);
            this.lblJitter.Name = "lblJitter";
            this.lblJitter.Size = new System.Drawing.Size(368, 42);
            this.lblJitter.TabIndex = 10;
            this.lblJitter.Text = "Jitter: -";
            // 
            // btnSaveUrl
            // 
            this.btnSaveUrl.Location = new System.Drawing.Point(326, 20);
            this.btnSaveUrl.Name = "btnSaveUrl";
            this.btnSaveUrl.Size = new System.Drawing.Size(88, 23);
            this.btnSaveUrl.TabIndex = 11;
            this.btnSaveUrl.Text = "URL Kayıt Et";
            this.btnSaveUrl.UseVisualStyleBackColor = true;
            this.btnSaveUrl.Click += new System.EventHandler(this.btnSaveUrl_Click);
            // 
            // linkLabelGitHub
            // 
            this.linkLabelGitHub.Location = new System.Drawing.Point(0, 0);
            this.linkLabelGitHub.Name = "linkLabelGitHub";
            this.linkLabelGitHub.Size = new System.Drawing.Size(100, 23);
            this.linkLabelGitHub.TabIndex = 0;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(703, 429);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(83, 13);
            this.linkLabel1.TabIndex = 12;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Github : Vuartex";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(818, 458);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.cmbInterval);
            this.Controls.Add(this.chkLog);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnSaveUrl);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.lblPingResult);
            this.Controls.Add(this.btnSpeedTest);
            this.Controls.Add(this.lblDownload);
            this.Controls.Add(this.lblUpload);
            this.Controls.Add(this.lblJitter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Ping ve Hız Testi Uygulaması Vuartex.0.1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

       


        private void btnStart_Click(object sender, EventArgs e)
        {

            // Eğer cooldown devam ediyorsa, yeniden başlatılmasına izin verme
            if (cooldownCounter > 0) return;

            // Cooldown başlat
            btnStart.Enabled = false;
            cooldownCounter = 2;
            btnStart.Text = $"({cooldownCounter})";
            cooldownTimer.Start();


            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Lütfen bir site adresi girin.");
                return;
            }


            double interval = GetIntervalMilliseconds();
            if (interval == 0)
            {
                MessageBox.Show("Lütfen geçerli bir aralık seçin.");
                return;
            }

            // Eğer chkLog işaretliyse, her testi başlatırken yeni dosya adı oluştur
            if (chkLog.Checked)
            {
                // Örnek: "14_02_2025_14_24" gibi
                string timeStr = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
                // Dosya adını: "14_02_2025_14_24_Ping_log.txt"
                string fileName = $"{timeStr}_Ping_log.txt";
                // Son olarak tam yol:
                logFilePath = Path.Combine(downloadsFolder, fileName);
            }
            else
            {
                // Log devrede değilse, logFilePath boş kalabilir
                logFilePath = null;
            }

            txtAddress.Enabled = false;
            btnSaveUrl.Enabled = false;
            btnOpenUrlManager.Enabled = false;
            toolTip.SetToolTip(txtAddress, "Test çalışırken URL değiştirilemez.");
            toolTip.SetToolTip(btnSaveUrl, "Test çalışırken URL kaydı yapılamaz.");
            // Timer kurulumu
            pingTimer = new System.Timers.Timer(interval);
            pingTimer.Elapsed += PingTimer_Elapsed;
            pingTimer.Start();

            PingOnce(); // Hemen başlat
        }

        private double GetIntervalMilliseconds()
        {
            switch (cmbInterval.SelectedItem?.ToString())
            {
                case "15s": return 15000;
                case "30s": return 30000;
                case "2dk": return 120000;
                case "5dk": return 300000;
                case "15dk": return 900000;
                case "30dk": return 1800000;
                case "1saat": return 3600000;
                default: return 0;
            }
        }
        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PingOnce();
        }

        private void PingOnce()
        {
            string site = txtAddress.InvokeRequired
                ? (string)txtAddress.Invoke(new Func<string>(() => txtAddress.Text))
                : txtAddress.Text;



            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd", $"/c ping {site} -n 1 -w 15000")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    ShowPingResult(output);

                    if (chkLog.Checked)
                        LogToFile(output);
                }
            }
            catch (Exception ex)
            {
                ShowPingResult("Hata: " + ex.Message);
            }
        }

        private void ShowPingResult(string result)
        {
            txtOutput.Invoke((MethodInvoker)(() =>
            {
                txtOutput.AppendText($"[{DateTime.Now}] {result}\r\n\r\n");

                if (result.Contains("ms"))
                    lblPingResult.Text = $"Ping başarılı: {ExtractPingTime(result)}";
                else
                    lblPingResult.Text = "Siteye ulaşılamadı.";
            }));
        }

        private string ExtractPingTime(string output)
        {
            int index = output.IndexOf("time=");
            if (index == -1) return "-";
            int end = output.IndexOf("ms", index);
            return output.Substring(index + 5, end - index - 5) + "ms";
        }

        private void LogToFile(string text)
        {
            File.AppendAllText((string)logFilePath, $"[{DateTime.Now}] {text}\r\n");
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (pingTimer != null)
            {
                pingTimer.Stop();
                pingTimer.Dispose();
                pingTimer = null;
            }
            lblPingResult.Text = "Test durduruldu.";

            txtAddress.Enabled = true;
            btnSaveUrl.Enabled = true;
            toolTip.SetToolTip(txtAddress, "");
            toolTip.SetToolTip(btnSaveUrl, "");
            btnOpenUrlManager.Enabled = true;
            // Log dosyası devredeyse
            if (chkLog.Checked && !string.IsNullOrEmpty(logFilePath))
            {
                // Son olarak ekrandaki her şeyi de ekle (isteğe bağlı)
                File.AppendAllText(logFilePath, "\n--- Test Durduruldu ---\n");
                File.AppendAllText(logFilePath, txtOutput.Text);

                // Kullanıcıya bildir
                MessageBox.Show($"Log dosyası oluşturuldu ve şu konuma kaydedildi:\n\n{logFilePath}",
                                "Bilgi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        private string GetSpeedtestExePath()
        {
            return Path.Combine(AppContext.BaseDirectory, "speedtest.exe");
        }


        private string ExtractSpeedtestExe()
        {
            // Proje adın VuartexPingMonitorApp, speedtest.exe projenin kökünde ise:
            string resourceName = "VuartexPingMonitorApp.speedtest.exe";
            // Yukarıdakini projende tam “embedded resource” ismiyle eşleştir

            string tempPath = Path.Combine(Path.GetTempPath(), "temp_speedtest.exe");

            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream resStream = asm.GetManifestResourceStream(resourceName))
            {
                if (resStream == null)
                    throw new Exception("speedtest.exe embedded resource not found. Check resource name!");

                byte[] buffer = new byte[resStream.Length];
                resStream.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(tempPath, buffer);
                // MessageBox.Show("Speedtest.exe çıkartıldı: " + tempPath);

            }

            return tempPath;
        }



        private void btnSaveUrl_Click(EventArgs e, object sender)
        {
            if (!string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show($"URL kaydedildi: {txtAddress.Text}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Kullanıcının girdiği URL'yi kaydet
               
            }
            else
            {
                MessageBox.Show("Lütfen bir URL giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private LinkLabel linkLabelGitHub;
        private LinkLabel linkLabel1;
    }
}

