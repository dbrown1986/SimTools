// SimTools
// Uninstaller
// Main Window Designer Code
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

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
            lblStatus = new System.Windows.Forms.Label();
            progOverall = new System.Windows.Forms.ProgressBar();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoEllipsis = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblStatus.Location = new System.Drawing.Point(20, 76);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(390, 20);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Initializing uninstaller...";
            // 
            // progOverall
            // 
            progOverall.Location = new System.Drawing.Point(20, 106);
            progOverall.Name = "progOverall";
            progOverall.Size = new System.Drawing.Size(390, 25);
            progOverall.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.plumbob;
            pictureBox1.Location = new System.Drawing.Point(417, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(120, 180);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // MainWindow
            // 
            ClientSize = new System.Drawing.Size(549, 203);
            Controls.Add(pictureBox1);
            Controls.Add(progOverall);
            Controls.Add(lblStatus);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SimTools Uninstaller";
            Load += MainWindow_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progOverall;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}