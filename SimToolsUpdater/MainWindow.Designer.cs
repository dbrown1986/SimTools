using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Font = System.Drawing.Font;

namespace SimToolsUpdater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            WelcomeScreen = new Panel();
            lblWelcomeBody = new Label();
            lblWelcomeTitle = new Label();
            UpdateScreen = new Panel();
            OverallProgressBar = new ProgressBar();
            lblOverallProgress = new Label();
            FileProgressBar = new ProgressBar();
            lblFileProgress = new Label();
            lblUpdatingTitle = new Label();
            CompleteScreen = new Panel();
            ChkLaunch = new CheckBox();
            lblCompleteBody = new Label();
            lblCompleteTitle = new Label();
            BottomPanel = new Panel();
            BtnCancel = new Button();
            BtnNext = new Button();
            pictureBox1 = new PictureBox();
            WelcomeScreen.SuspendLayout();
            UpdateScreen.SuspendLayout();
            CompleteScreen.SuspendLayout();
            BottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // WelcomeScreen
            // 
            WelcomeScreen.Controls.Add(lblWelcomeBody);
            WelcomeScreen.Controls.Add(lblWelcomeTitle);
            WelcomeScreen.Location = new Point(12, 12);
            WelcomeScreen.Name = "WelcomeScreen";
            WelcomeScreen.Size = new Size(510, 280);
            WelcomeScreen.TabIndex = 0;
            // 
            // lblWelcomeBody
            // 
            lblWelcomeBody.Font = new Font("Segoe UI", 10F);
            lblWelcomeBody.ForeColor = Color.FromArgb(50, 50, 50);
            lblWelcomeBody.Location = new Point(10, 80);
            lblWelcomeBody.Name = "lblWelcomeBody";
            lblWelcomeBody.Size = new Size(490, 180);
            lblWelcomeBody.TabIndex = 0;
            lblWelcomeBody.Text = "The Update Wizard will update the existing version of SimTools on your computer to the latest available release.\r\n\r\nClick Next to continue or Cancel to exit the Setup Wizard.";
            // 
            // lblWelcomeTitle
            // 
            lblWelcomeTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblWelcomeTitle.Location = new Point(10, 20);
            lblWelcomeTitle.Name = "lblWelcomeTitle";
            lblWelcomeTitle.Size = new Size(490, 40);
            lblWelcomeTitle.TabIndex = 1;
            lblWelcomeTitle.Text = "Welcome to the SimTools Update Wizard";
            // 
            // UpdateScreen
            // 
            UpdateScreen.Controls.Add(OverallProgressBar);
            UpdateScreen.Controls.Add(lblOverallProgress);
            UpdateScreen.Controls.Add(FileProgressBar);
            UpdateScreen.Controls.Add(lblFileProgress);
            UpdateScreen.Controls.Add(lblUpdatingTitle);
            UpdateScreen.Location = new Point(12, 12);
            UpdateScreen.Name = "UpdateScreen";
            UpdateScreen.Size = new Size(510, 280);
            UpdateScreen.TabIndex = 1;
            UpdateScreen.Visible = false;
            // 
            // OverallProgressBar
            // 
            OverallProgressBar.Location = new Point(10, 175);
            OverallProgressBar.Name = "OverallProgressBar";
            OverallProgressBar.Size = new Size(490, 23);
            OverallProgressBar.TabIndex = 0;
            // 
            // lblOverallProgress
            // 
            lblOverallProgress.Location = new Point(10, 150);
            lblOverallProgress.Name = "lblOverallProgress";
            lblOverallProgress.Size = new Size(490, 20);
            lblOverallProgress.TabIndex = 1;
            lblOverallProgress.Text = "Overall Progress:";
            // 
            // FileProgressBar
            // 
            FileProgressBar.Location = new Point(10, 105);
            FileProgressBar.Name = "FileProgressBar";
            FileProgressBar.Size = new Size(490, 18);
            FileProgressBar.TabIndex = 2;
            // 
            // lblFileProgress
            // 
            lblFileProgress.Location = new Point(10, 80);
            lblFileProgress.Name = "lblFileProgress";
            lblFileProgress.Size = new Size(490, 20);
            lblFileProgress.TabIndex = 3;
            lblFileProgress.Text = "File Progress:";
            // 
            // lblUpdatingTitle
            // 
            lblUpdatingTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblUpdatingTitle.Location = new Point(10, 20);
            lblUpdatingTitle.Name = "lblUpdatingTitle";
            lblUpdatingTitle.Size = new Size(490, 40);
            lblUpdatingTitle.TabIndex = 4;
            lblUpdatingTitle.Text = "Updating...";
            // 
            // CompleteScreen
            // 
            CompleteScreen.Controls.Add(ChkLaunch);
            CompleteScreen.Controls.Add(lblCompleteBody);
            CompleteScreen.Controls.Add(lblCompleteTitle);
            CompleteScreen.Location = new Point(12, 12);
            CompleteScreen.Name = "CompleteScreen";
            CompleteScreen.Size = new Size(510, 280);
            CompleteScreen.TabIndex = 2;
            CompleteScreen.Visible = false;
            // 
            // ChkLaunch
            // 
            ChkLaunch.CheckState = CheckState.Checked;
            ChkLaunch.Checked = true;
            ChkLaunch.Font = new Font("Segoe UI", 10F);
            ChkLaunch.Location = new Point(14, 160);
            ChkLaunch.Name = "ChkLaunch";
            ChkLaunch.Size = new Size(200, 24);
            ChkLaunch.TabIndex = 0;
            ChkLaunch.Text = "Launch SimTools now";
            // 
            // lblCompleteBody
            // 
            lblCompleteBody.Font = new Font("Segoe UI", 10F);
            lblCompleteBody.ForeColor = Color.FromArgb(50, 50, 50);
            lblCompleteBody.Location = new Point(10, 80);
            lblCompleteBody.Name = "lblCompleteBody";
            lblCompleteBody.Size = new Size(490, 60);
            lblCompleteBody.TabIndex = 1;
            lblCompleteBody.Text = "SimTools has been successfully updated on your computer.";
            // 
            // lblCompleteTitle
            // 
            lblCompleteTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblCompleteTitle.Location = new Point(10, 20);
            lblCompleteTitle.Name = "lblCompleteTitle";
            lblCompleteTitle.Size = new Size(490, 40);
            lblCompleteTitle.TabIndex = 2;
            lblCompleteTitle.Text = "Completing the SimTools Setup Wizard";
            // 
            // BottomPanel
            // 
            BottomPanel.Controls.Add(BtnCancel);
            BottomPanel.Controls.Add(BtnNext);
            BottomPanel.Dock = DockStyle.Bottom;
            BottomPanel.Location = new Point(0, 305);
            BottomPanel.Name = "BottomPanel";
            BottomPanel.Size = new Size(696, 56);
            BottomPanel.TabIndex = 3;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(425, 12);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(95, 30);
            BtnCancel.TabIndex = 0;
            BtnCancel.Text = "Cancel";
            BtnCancel.Click += BtnCancel_Click;
            // 
            // BtnNext
            // 
            BtnNext.Location = new Point(320, 12);
            BtnNext.Name = "BtnNext";
            BtnNext.Size = new Size(95, 30);
            BtnNext.TabIndex = 1;
            BtnNext.Text = "Next >";
            BtnNext.Click += BtnNext_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.plumbob;
            pictureBox1.Location = new Point(562, 59);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(122, 181);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 243, 243);
            ClientSize = new Size(696, 361);
            Controls.Add(pictureBox1);
            Controls.Add(WelcomeScreen);
            Controls.Add(UpdateScreen);
            Controls.Add(CompleteScreen);
            Controls.Add(BottomPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SimTools Update Wizard";
            WelcomeScreen.ResumeLayout(false);
            UpdateScreen.ResumeLayout(false);
            CompleteScreen.ResumeLayout(false);
            BottomPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private Panel WelcomeScreen;
        private Label lblWelcomeTitle;
        private Label lblWelcomeBody;
        private Panel UpdateScreen;
        private Label lblUpdatingTitle;
        private Label lblFileProgress;
        private ProgressBar FileProgressBar;
        private Label lblOverallProgress;
        private ProgressBar OverallProgressBar;
        private Panel CompleteScreen;
        private Label lblCompleteTitle;
        private Label lblCompleteBody;
        private CheckBox ChkLaunch;
        private Panel BottomPanel;
        private Button BtnNext;
        private Button BtnCancel;
        private PictureBox pictureBox1;
    }
}