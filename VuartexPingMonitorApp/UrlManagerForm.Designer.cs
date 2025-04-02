namespace VuartexPingMonitorApp
{
    partial class UrlManagerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lstUrls;
        private System.Windows.Forms.TextBox txtNewUrl;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUseSelected;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstUrls = new System.Windows.Forms.ListBox();
            this.txtNewUrl = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnUseSelected = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lstUrls.Location = new System.Drawing.Point(20, 20);
            this.lstUrls.Size = new System.Drawing.Size(300, 150);

            this.txtNewUrl.Location = new System.Drawing.Point(20, 180);
            this.txtNewUrl.Size = new System.Drawing.Size(300, 20);

            this.btnAdd.Location = new System.Drawing.Point(330, 180);
            this.btnAdd.Text = "Ekle";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);

            this.btnDelete.Location = new System.Drawing.Point(20, 210);
            this.btnDelete.Text = "Seçiliyi Sil";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            this.btnUseSelected.Location = new System.Drawing.Point(120, 210);
            this.btnUseSelected.Size = new System.Drawing.Size(120, 23);
            this.btnUseSelected.Text = "Seçiliyi Kullan";
            this.btnUseSelected.Click += new System.EventHandler(this.btnUseSelected_Click);

            this.ClientSize = new System.Drawing.Size(400, 260);
            this.Controls.Add(this.lstUrls);
            this.Controls.Add(this.txtNewUrl);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnUseSelected);
            this.Text = "URL Yöneticisi";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
