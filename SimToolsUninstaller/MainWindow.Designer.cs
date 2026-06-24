namespace SimToolsUninstaller
{
    partial class MainWindow
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblStatus = new System.Windows.Forms.Label();
            this.progOverall = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(20, 20);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(390, 20);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Initializing uninstaller...";
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // progOverall
            this.progOverall.Location = new System.Drawing.Point(20, 50);
            this.progOverall.Name = "progOverall";
            this.progOverall.Size = new System.Drawing.Size(390, 25);
            this.progOverall.TabIndex = 1;

            // MainWindow
            this.ClientSize = new System.Drawing.Size(430, 100);
            this.Controls.Add(this.progOverall);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SimTools Uninstaller";

            // Hooks up the logic when the window opens
            this.Load += new System.EventHandler(this.MainWindow_Load);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progOverall;
    }
}