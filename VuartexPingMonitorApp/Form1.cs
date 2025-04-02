using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Policy;

namespace VuartexPingMonitorApp
{
    public partial class Form1 : Form
    {


        private string? url;
        private System.Windows.Forms.Timer cooldownTimer;
        private int cooldownCounter = 0;

        // Ping testi timer'ı, log dosyası vs. .Designer.cs'de tanımlanmış. 
        // Burada constructor ve metodlarımız var.

        public Form1()
        {
          
            InitializeComponent();
            this.Load += Form1_Load;
         

            // Son kaydedilen URL


            // Basit Placeholder mantığı
            txtAddress.ForeColor = Color.Gray;
            txtAddress.Text = "Bir URL yazınız ve Kayıt Et tuşuna basınız";

            txtAddress.GotFocus += (s, e) =>
            {
                if (txtAddress.Text == "Bir URL yazınız ve Kayıt Et tuşuna basınız")
                {
                    txtAddress.Text = "";
                    txtAddress.ForeColor = Color.Black;
                }
            };
            txtAddress.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    txtAddress.Text = "Bir URL yazınız ve Kayıt Et tuşuna basınız";
                    txtAddress.ForeColor = Color.Gray;
                }
            };

            string savedUrl = LoadLastUrl();
            if (!string.IsNullOrEmpty(savedUrl))
            {
                txtAddress.Text = savedUrl;
            }
        }

        private string LoadLastUrl()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "user_settings.txt");
            if (File.Exists(path))
                return File.ReadAllText(path);
            return string.Empty;
        }

        private void EnsureSpeedtestLicenseAccepted()
        {
            string ooklaDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ookla");
            Directory.CreateDirectory(ooklaDir);

            string licenseFile = Path.Combine(ooklaDir, "speedtest-acceptance.json");

            if (!File.Exists(licenseFile))
            {
                string json = "{ \"acceptance\": \"YES\" }";
                File.WriteAllText(licenseFile, json);
            }
        }

        private void btnOpenUrlManager_Click(object sender, EventArgs e)
        {
            var urlForm = new UrlManagerForm();
            urlForm.UrlSelected += (selectedUrl) => { txtAddress.Text = selectedUrl; };
            urlForm.ShowDialog();
        }

        // =====================================
        // 1) SpeedTest CLI (Anlık Mod)
        // =====================================

        private async void btnSpeedTest_Click(object sender, EventArgs e)
        {


            btnSpeedTest.Enabled = false;
            lblDownload.Text = "Download: Ölçüm yapılıyor... Bu işlem yaklaşık 2 dakika sürebilir.";
            lblUpload.Text = "Upload: Ölçüm yapılıyor... Bu işlem yaklaşık 2 dakika sürebilir.";
            lblJitter.Text = "Jitter: Ölçüm yapılıyor... Bu işlem yaklaşık 2 dakika sürebilir. ";
            txtOutput.Text = "";

            try
            {
                string extractedExePath = ExtractSpeedtestExe();
                string outputFile = Path.Combine(Path.GetTempPath(), "speedtest_output.txt");

                var psi = new ProcessStartInfo
                {
                    FileName = extractedExePath,
                    Arguments = "--accept-license -f json",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                string result = "";
                using (var process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();
                    result = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();
                }

                var json = System.Text.Json.JsonDocument.Parse(result).RootElement;

             
                // Download
                if (json.TryGetProperty("download", out var download) &&
                    download.TryGetProperty("bandwidth", out var dlBw))
                {
                    double dlMbps = dlBw.GetDouble() * 8 / 1_000_000;
                    lblDownload.Invoke(new MethodInvoker(() => lblDownload.Text = $"Download: {dlMbps:F2} Mbps"));
                }

                // Upload
                if (json.TryGetProperty("upload", out var upload) &&
                    upload.TryGetProperty("bandwidth", out var ulBw))
                {
                    double ulMbps = ulBw.GetDouble() * 8 / 1_000_000;
                    lblUpload.Invoke(new MethodInvoker(() => lblUpload.Text = $"Upload: {ulMbps:F2} Mbps"));

                }

                // Jitter
                if (json.TryGetProperty("ping", out var ping) &&
                    ping.TryGetProperty("jitter", out var jitter))
                {
                    double jitterVal = jitter.GetDouble();
                    lblJitter.Invoke(new MethodInvoker(() => lblJitter.Text = $"Jitter: {jitterVal:F2} ms"));
                }


                File.Delete(outputFile);
                File.Delete(extractedExePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hız testi başlatılamadı: " + ex.Message);
            }

            btnSpeedTest.Enabled = true;
        }




        // Speedtest CLI satırlarını parse ediyoruz
        private void ParseSpeedtestLine(string line)
        {
            // 1) Hata var mı?
            if (line.StartsWith("[error]"))
            {
                this.Invoke(new Action(() =>
                {
                    lblJitter.Text = "Hata: " + line;
                }));
            }
            // 2) "Download:   189.08 Mbps (data used: ...)"
            else if (line.Trim().StartsWith("Download:"))
            {
                // "Download:   189.08 Mbps ..."
                // sayıyı çek
                string downloadStr = ExtractBetween(line, "Download:", "Mbps");
                if (!string.IsNullOrWhiteSpace(downloadStr))
                {
                    downloadStr = downloadStr.Trim();
                    // → InvariantCulture kullanıyoruz
                    if (double.TryParse(downloadStr,
                                       NumberStyles.Any,
                                       CultureInfo.InvariantCulture,
                                       out double dlMbps))
                    {
                        this.Invoke(new Action(() =>
                        {
                            lblDownload.Text = $"Download: {dlMbps:F2} Mbps";
                        }));
                    }
                }
            }
            // 3) "Upload:   308.44 Mbps [=\\ ]  8%   - latency: 58.74 ms"
            else if (line.Trim().StartsWith("Upload:"))
            {
                string upStr = ExtractBetween(line, "Upload:", "Mbps");
                if (!string.IsNullOrWhiteSpace(upStr))
                {
                    upStr = upStr.Trim();
                    if (double.TryParse(upStr,
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out double ulMbps))
                    {
                        this.Invoke(new Action(() =>
                        {
                            lblUpload.Text = $"Upload: {ulMbps:F2} Mbps";
                        }));
                    }
                }

                // Yüzde parse
                int idxPercent = line.IndexOf('%');
                if (idxPercent > 0)
                {
                    int idxSpace = line.LastIndexOf(' ', idxPercent);
                    if (idxSpace > 0)
                    {
                        string percentStr = line.Substring(idxSpace, idxPercent - idxSpace).Trim();
                        if (double.TryParse(percentStr,
                                            NumberStyles.Any,
                                            CultureInfo.InvariantCulture,
                                            out double progress))
                        {
                            this.Invoke(new Action(() =>
                            {
                                lblUpload.Text += $"  ({progress:F0}% anlık)";
                            }));
                        }
                    }
                }
            }
            // 4) Jitter içeren satırlar
            // Ör: "Idle Latency:    22.04 ms   (jitter: 2.90ms, low: 21.02ms, high: 27.06ms)"
            // veya "339.65 ms   (jitter: 82.20ms, low: 24.86ms, high: 634.51ms)"
            else if (line.Contains("(jitter:"))
            {
                // "jitter: 2.90ms"
                string jitterStr = ExtractBetween(line, "(jitter:", "ms");
                if (!string.IsNullOrWhiteSpace(jitterStr))
                {
                    jitterStr = jitterStr.Trim();
                    if (double.TryParse(jitterStr,
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out double jVal))
                    {
                        this.Invoke(new Action(() =>
                        {
                            lblJitter.Text = $"Jitter: {jVal:F2} ms";
                        }));
                    }
                }
            }
            // vb. Diğer satırlar
        }

        private string ExtractBetween(string input, string start, string end)
        {
            int startPos = input.IndexOf(start);
            if (startPos < 0) return "";
            startPos += start.Length;
            int endPos = input.IndexOf(end, startPos);
            if (endPos < 0) return "";
            return input.Substring(startPos, endPos - startPos);
        }


        // URL Kaydet
        private void btnSaveUrl_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show($"URL kaydedildi: {txtAddress.Text}");
                SaveLastUrl(txtAddress.Text);
            }
            else
            {
                MessageBox.Show("Lütfen bir URL giriniz.");
            }
        }

      

        private void SaveLastUrl(string text)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "user_settings.txt");
            File.WriteAllText(path, text);

        }

        // Kullanıcı Interval seçti
        private void cmbInterval_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtAddress_TextChanged(object sender, EventArgs e) { }

        private void linkLabelGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Modern yöntemde:
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/vuartex",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Modern yöntemde:
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/vuartex",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }







}
