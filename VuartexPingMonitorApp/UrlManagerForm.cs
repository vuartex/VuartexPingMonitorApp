using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace VuartexPingMonitorApp
{


  
    public partial class UrlManagerForm : Form
    {
        public event Action<string>? UrlSelected;
        private string urlsFilePath;

        public UrlManagerForm()
        {
            InitializeComponent();
            urlsFilePath = Path.Combine(AppContext.BaseDirectory, "saved_urls.txt");
            LoadUrls();
        }

        private void LoadUrls()
        {
            lstUrls.Items.Clear();
            if (File.Exists(urlsFilePath))
            {
                foreach (var line in File.ReadAllLines(urlsFilePath))
                {
                    lstUrls.Items.Add(line);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var url = txtNewUrl.Text.Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                File.AppendAllText(urlsFilePath, url + Environment.NewLine);
                LoadUrls();
                txtNewUrl.Text = "";
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            var selected = lstUrls.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(selected))
            {
                var allUrls = new List<string>(File.ReadAllLines(urlsFilePath));
                allUrls.Remove(selected);
                File.WriteAllLines(urlsFilePath, allUrls);
                LoadUrls();
            }
        }

        private void btnUseSelected_Click(object sender, EventArgs e)
        {
            if (lstUrls.SelectedItem != null)
            {
                UrlSelected?.Invoke(lstUrls.SelectedItem.ToString());
                this.Close();
            }
        }
    }
}
