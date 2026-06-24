namespace SimToolsInstaller
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
            pnlBottom = new System.Windows.Forms.Panel();
            btnPrev = new System.Windows.Forms.Button();
            btnNext = new System.Windows.Forms.Button();
            btnCancelExit = new System.Windows.Forms.Button();
            wizardTabs = new System.Windows.Forms.TabControl();
            tabWelcome = new System.Windows.Forms.TabPage();
            PlumbobPictureBox = new System.Windows.Forms.PictureBox();
            lblWelcomeTitle = new System.Windows.Forms.Label();
            lblWelcomeDesc = new System.Windows.Forms.Label();
            tabLicense = new System.Windows.Forms.TabPage();
            PlumbobPictureBox2 = new System.Windows.Forms.PictureBox();
            lblLicenseTitle = new System.Windows.Forms.Label();
            lblLicenseDesc = new System.Windows.Forms.Label();
            txtLicense = new System.Windows.Forms.TextBox();
            radAgree = new System.Windows.Forms.RadioButton();
            radDisagree = new System.Windows.Forms.RadioButton();
            tabArch = new System.Windows.Forms.TabPage();
            richTextBox2 = new System.Windows.Forms.RichTextBox();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            lblArchTitle = new System.Windows.Forms.Label();
            lblArchDesc = new System.Windows.Forms.Label();
            rad64Bit = new System.Windows.Forms.RadioButton();
            rad32Bit = new System.Windows.Forms.RadioButton();
            tabDirectory = new System.Windows.Forms.TabPage();
            pictureBox5 = new System.Windows.Forms.PictureBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            lblDirTitle = new System.Windows.Forms.Label();
            lblDirDesc = new System.Windows.Forms.Label();
            txtInstallPath = new System.Windows.Forms.TextBox();
            btnBrowse = new System.Windows.Forms.Button();
            chkDesktop = new System.Windows.Forms.CheckBox();
            chkStartMenu = new System.Windows.Forms.CheckBox();
            tabReady = new System.Windows.Forms.TabPage();
            pictureBox7 = new System.Windows.Forms.PictureBox();
            lblReadyTitle = new System.Windows.Forms.Label();
            lblReadyDesc = new System.Windows.Forms.Label();
            tabInstall = new System.Windows.Forms.TabPage();
            pictureBox9 = new System.Windows.Forms.PictureBox();
            lblInstallTitle = new System.Windows.Forms.Label();
            lblCurrentFile = new System.Windows.Forms.Label();
            progCurrent = new System.Windows.Forms.ProgressBar();
            lblOverall = new System.Windows.Forms.Label();
            progOverall = new System.Windows.Forms.ProgressBar();
            tabInterrupt = new System.Windows.Forms.TabPage();
            pictureBox11 = new System.Windows.Forms.PictureBox();
            label2 = new System.Windows.Forms.Label();
            lblInterruptTitle = new System.Windows.Forms.Label();
            lblInterruptDesc = new System.Windows.Forms.Label();
            tabComplete = new System.Windows.Forms.TabPage();
            pictureBox13 = new System.Windows.Forms.PictureBox();
            lblCompleteTitle = new System.Windows.Forms.Label();
            lblCompleteDesc = new System.Windows.Forms.Label();
            chkRun = new System.Windows.Forms.CheckBox();
            pnlBottom.SuspendLayout();
            wizardTabs.SuspendLayout();
            tabWelcome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PlumbobPictureBox).BeginInit();
            tabLicense.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PlumbobPictureBox2).BeginInit();
            tabArch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            tabDirectory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            tabReady.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            tabInstall.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
            tabInterrupt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).BeginInit();
            tabComplete.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox13).BeginInit();
            SuspendLayout();
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = System.Drawing.Color.CornflowerBlue;
            pnlBottom.Controls.Add(btnPrev);
            pnlBottom.Controls.Add(btnNext);
            pnlBottom.Controls.Add(btnCancelExit);
            pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlBottom.Location = new System.Drawing.Point(0, 400);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new System.Drawing.Size(600, 50);
            pnlBottom.TabIndex = 1;
            // 
            // btnPrev
            // 
            btnPrev.ForeColor = System.Drawing.Color.FloralWhite;
            btnPrev.Location = new System.Drawing.Point(320, 10);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new System.Drawing.Size(75, 25);
            btnPrev.TabIndex = 0;
            btnPrev.Text = "< Back";
            // 
            // btnNext
            // 
            btnNext.ForeColor = System.Drawing.Color.FloralWhite;
            btnNext.Location = new System.Drawing.Point(405, 10);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(75, 25);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next >";
            // 
            // btnCancelExit
            // 
            btnCancelExit.ForeColor = System.Drawing.Color.FloralWhite;
            btnCancelExit.Location = new System.Drawing.Point(490, 10);
            btnCancelExit.Name = "btnCancelExit";
            btnCancelExit.Size = new System.Drawing.Size(75, 25);
            btnCancelExit.TabIndex = 2;
            btnCancelExit.Text = "Cancel";
            // 
            // wizardTabs
            // 
            wizardTabs.Controls.Add(tabWelcome);
            wizardTabs.Controls.Add(tabLicense);
            wizardTabs.Controls.Add(tabArch);
            wizardTabs.Controls.Add(tabDirectory);
            wizardTabs.Controls.Add(tabReady);
            wizardTabs.Controls.Add(tabInstall);
            wizardTabs.Controls.Add(tabInterrupt);
            wizardTabs.Controls.Add(tabComplete);
            wizardTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            wizardTabs.Location = new System.Drawing.Point(0, 0);
            wizardTabs.Name = "wizardTabs";
            wizardTabs.SelectedIndex = 0;
            wizardTabs.Size = new System.Drawing.Size(600, 400);
            wizardTabs.TabIndex = 0;
            // 
            // tabWelcome
            // 
            tabWelcome.BackColor = System.Drawing.Color.CornflowerBlue;
            tabWelcome.Controls.Add(PlumbobPictureBox);
            tabWelcome.Controls.Add(lblWelcomeTitle);
            tabWelcome.Controls.Add(lblWelcomeDesc);
            tabWelcome.Location = new System.Drawing.Point(4, 24);
            tabWelcome.Name = "tabWelcome";
            tabWelcome.Padding = new System.Windows.Forms.Padding(20);
            tabWelcome.Size = new System.Drawing.Size(592, 372);
            tabWelcome.TabIndex = 0;
            tabWelcome.Text = "1. Welcome";
            // 
            // PlumbobPictureBox
            // 
            PlumbobPictureBox.Image = Properties.Resources.plumbob1;
            PlumbobPictureBox.Location = new System.Drawing.Point(8, 93);
            PlumbobPictureBox.Name = "PlumbobPictureBox";
            PlumbobPictureBox.Size = new System.Drawing.Size(120, 180);
            PlumbobPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            PlumbobPictureBox.TabIndex = 2;
            PlumbobPictureBox.TabStop = false;
            // 
            // lblWelcomeTitle
            // 
            lblWelcomeTitle.AutoSize = true;
            lblWelcomeTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblWelcomeTitle.ForeColor = System.Drawing.Color.FloralWhite;
            lblWelcomeTitle.Location = new System.Drawing.Point(146, 20);
            lblWelcomeTitle.Name = "lblWelcomeTitle";
            lblWelcomeTitle.Size = new System.Drawing.Size(421, 30);
            lblWelcomeTitle.TabIndex = 0;
            lblWelcomeTitle.Text = "Welcome to the SimTools Setup Wizard";
            // 
            // lblWelcomeDesc
            // 
            lblWelcomeDesc.ForeColor = System.Drawing.Color.FloralWhite;
            lblWelcomeDesc.Location = new System.Drawing.Point(146, 93);
            lblWelcomeDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblWelcomeDesc.Name = "lblWelcomeDesc";
            lblWelcomeDesc.Size = new System.Drawing.Size(415, 200);
            lblWelcomeDesc.TabIndex = 1;
            lblWelcomeDesc.Text = resources.GetString("lblWelcomeDesc.Text");
            // 
            // tabLicense
            // 
            tabLicense.BackColor = System.Drawing.Color.CornflowerBlue;
            tabLicense.Controls.Add(PlumbobPictureBox2);
            tabLicense.Controls.Add(lblLicenseTitle);
            tabLicense.Controls.Add(lblLicenseDesc);
            tabLicense.Controls.Add(txtLicense);
            tabLicense.Controls.Add(radAgree);
            tabLicense.Controls.Add(radDisagree);
            tabLicense.ForeColor = System.Drawing.Color.FloralWhite;
            tabLicense.Location = new System.Drawing.Point(4, 24);
            tabLicense.Name = "tabLicense";
            tabLicense.Padding = new System.Windows.Forms.Padding(20);
            tabLicense.Size = new System.Drawing.Size(592, 372);
            tabLicense.TabIndex = 1;
            tabLicense.Text = "2. License";
            // 
            // PlumbobPictureBox2
            // 
            PlumbobPictureBox2.Image = Properties.Resources.plumbob1;
            PlumbobPictureBox2.Location = new System.Drawing.Point(8, 93);
            PlumbobPictureBox2.Name = "PlumbobPictureBox2";
            PlumbobPictureBox2.Size = new System.Drawing.Size(120, 180);
            PlumbobPictureBox2.TabIndex = 5;
            PlumbobPictureBox2.TabStop = false;
            // 
            // lblLicenseTitle
            // 
            lblLicenseTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblLicenseTitle.Location = new System.Drawing.Point(138, 20);
            lblLicenseTitle.Name = "lblLicenseTitle";
            lblLicenseTitle.Size = new System.Drawing.Size(210, 30);
            lblLicenseTitle.TabIndex = 0;
            lblLicenseTitle.Text = "License Agreement";
            // 
            // lblLicenseDesc
            // 
            lblLicenseDesc.AutoSize = true;
            lblLicenseDesc.Location = new System.Drawing.Point(138, 59);
            lblLicenseDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblLicenseDesc.Name = "lblLicenseDesc";
            lblLicenseDesc.Size = new System.Drawing.Size(306, 15);
            lblLicenseDesc.TabIndex = 1;
            lblLicenseDesc.Text = "Please review the license terms before installing SimTools.";
            // 
            // txtLicense
            // 
            txtLicense.Location = new System.Drawing.Point(138, 93);
            txtLicense.Multiline = true;
            txtLicense.Name = "txtLicense";
            txtLicense.ReadOnly = true;
            txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLicense.Size = new System.Drawing.Size(422, 213);
            txtLicense.TabIndex = 2;
            // 
            // radAgree
            // 
            radAgree.AutoSize = true;
            radAgree.Location = new System.Drawing.Point(138, 312);
            radAgree.Name = "radAgree";
            radAgree.Size = new System.Drawing.Size(126, 19);
            radAgree.TabIndex = 3;
            radAgree.Text = "I agree to the terms";
            // 
            // radDisagree
            // 
            radDisagree.AutoSize = true;
            radDisagree.Checked = true;
            radDisagree.Location = new System.Drawing.Point(138, 337);
            radDisagree.Name = "radDisagree";
            radDisagree.Size = new System.Drawing.Size(98, 19);
            radDisagree.TabIndex = 4;
            radDisagree.TabStop = true;
            radDisagree.Text = "I do not agree";
            // 
            // tabArch
            // 
            tabArch.BackColor = System.Drawing.Color.CornflowerBlue;
            tabArch.Controls.Add(richTextBox2);
            tabArch.Controls.Add(richTextBox1);
            tabArch.Controls.Add(pictureBox3);
            tabArch.Controls.Add(lblArchTitle);
            tabArch.Controls.Add(lblArchDesc);
            tabArch.Controls.Add(rad64Bit);
            tabArch.Controls.Add(rad32Bit);
            tabArch.ForeColor = System.Drawing.Color.FloralWhite;
            tabArch.Location = new System.Drawing.Point(4, 24);
            tabArch.Name = "tabArch";
            tabArch.Padding = new System.Windows.Forms.Padding(20);
            tabArch.Size = new System.Drawing.Size(592, 372);
            tabArch.TabIndex = 2;
            tabArch.Text = "3. Architecture";
            // 
            // richTextBox2
            // 
            richTextBox2.BackColor = System.Drawing.Color.CornflowerBlue;
            richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBox2.ForeColor = System.Drawing.Color.FloralWhite;
            richTextBox2.Location = new System.Drawing.Point(161, 222);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new System.Drawing.Size(378, 71);
            richTextBox2.TabIndex = 10;
            richTextBox2.Text = resources.GetString("richTextBox2.Text");
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = System.Drawing.Color.CornflowerBlue;
            richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBox1.ForeColor = System.Drawing.Color.FloralWhite;
            richTextBox1.Location = new System.Drawing.Point(162, 123);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(377, 68);
            richTextBox1.TabIndex = 9;
            richTextBox1.Text = "The 32-Bit version of SimTools is ideal for legacy systems running on a quad-core CPU or below with 4GB - 8GB of system memory. 32-Bit system executables are restricted to only 4GB~ max usage.";
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.plumbob1;
            pictureBox3.Location = new System.Drawing.Point(8, 93);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(120, 180);
            pictureBox3.TabIndex = 7;
            pictureBox3.TabStop = false;
            // 
            // lblArchTitle
            // 
            lblArchTitle.AutoSize = true;
            lblArchTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblArchTitle.Location = new System.Drawing.Point(144, 20);
            lblArchTitle.Name = "lblArchTitle";
            lblArchTitle.Size = new System.Drawing.Size(206, 30);
            lblArchTitle.TabIndex = 0;
            lblArchTitle.Text = "Select Architecture";
            // 
            // lblArchDesc
            // 
            lblArchDesc.AutoSize = true;
            lblArchDesc.Location = new System.Drawing.Point(144, 60);
            lblArchDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblArchDesc.Name = "lblArchDesc";
            lblArchDesc.Size = new System.Drawing.Size(392, 15);
            lblArchDesc.TabIndex = 1;
            lblArchDesc.Text = "Please select the version of SimTools you wish to install based on your OS.";
            // 
            // rad64Bit
            // 
            rad64Bit.AutoSize = true;
            rad64Bit.Location = new System.Drawing.Point(144, 197);
            rad64Bit.Name = "rad64Bit";
            rad64Bit.Size = new System.Drawing.Size(86, 19);
            rad64Bit.TabIndex = 2;
            rad64Bit.Text = "64-Bit (x64)";
            // 
            // rad32Bit
            // 
            rad32Bit.AutoSize = true;
            rad32Bit.Location = new System.Drawing.Point(144, 98);
            rad32Bit.Name = "rad32Bit";
            rad32Bit.Size = new System.Drawing.Size(84, 19);
            rad32Bit.TabIndex = 3;
            rad32Bit.Text = "32-Bit (x86)";
            // 
            // tabDirectory
            // 
            tabDirectory.BackColor = System.Drawing.Color.CornflowerBlue;
            tabDirectory.Controls.Add(pictureBox5);
            tabDirectory.Controls.Add(checkBox2);
            tabDirectory.Controls.Add(checkBox1);
            tabDirectory.Controls.Add(label1);
            tabDirectory.Controls.Add(lblDirTitle);
            tabDirectory.Controls.Add(lblDirDesc);
            tabDirectory.Controls.Add(txtInstallPath);
            tabDirectory.Controls.Add(btnBrowse);
            tabDirectory.Controls.Add(chkDesktop);
            tabDirectory.Controls.Add(chkStartMenu);
            tabDirectory.ForeColor = System.Drawing.Color.FloralWhite;
            tabDirectory.Location = new System.Drawing.Point(4, 24);
            tabDirectory.Name = "tabDirectory";
            tabDirectory.Padding = new System.Windows.Forms.Padding(20);
            tabDirectory.Size = new System.Drawing.Size(592, 372);
            tabDirectory.TabIndex = 3;
            tabDirectory.Text = "4. Directory";
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.plumbob1;
            pictureBox5.Location = new System.Drawing.Point(8, 93);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new System.Drawing.Size(120, 180);
            pictureBox5.TabIndex = 9;
            pictureBox5.TabStop = false;
            // 
            // checkBox2
            // 
            checkBox2.Enabled = false;
            checkBox2.Location = new System.Drawing.Point(146, 297);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(265, 19);
            checkBox2.TabIndex = 8;
            checkBox2.Text = "Also Install Language Tool (In Development)";
            // 
            // checkBox1
            // 
            checkBox1.Enabled = false;
            checkBox1.Location = new System.Drawing.Point(146, 267);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(276, 19);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "Also Install Repo Maker Utility (In Development)";
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(146, 87);
            label1.MaximumSize = new System.Drawing.Size(540, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(411, 63);
            label1.TabIndex = 6;
            label1.Text = resources.GetString("label1.Text");
            // 
            // lblDirTitle
            // 
            lblDirTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblDirTitle.Location = new System.Drawing.Point(146, 20);
            lblDirTitle.Name = "lblDirTitle";
            lblDirTitle.Size = new System.Drawing.Size(265, 30);
            lblDirTitle.TabIndex = 0;
            lblDirTitle.Text = "Choose Install Location";
            // 
            // lblDirDesc
            // 
            lblDirDesc.Location = new System.Drawing.Point(146, 60);
            lblDirDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblDirDesc.Name = "lblDirDesc";
            lblDirDesc.Size = new System.Drawing.Size(317, 18);
            lblDirDesc.TabIndex = 1;
            lblDirDesc.Text = "Choose the folder in which to install SimTools.";
            // 
            // txtInstallPath
            // 
            txtInstallPath.Location = new System.Drawing.Point(146, 166);
            txtInstallPath.Name = "txtInstallPath";
            txtInstallPath.Size = new System.Drawing.Size(327, 23);
            txtInstallPath.TabIndex = 2;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new System.Drawing.Point(486, 164);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(72, 26);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "Browse...";
            // 
            // chkDesktop
            // 
            chkDesktop.Checked = true;
            chkDesktop.CheckState = System.Windows.Forms.CheckState.Checked;
            chkDesktop.Location = new System.Drawing.Point(146, 206);
            chkDesktop.Name = "chkDesktop";
            chkDesktop.Size = new System.Drawing.Size(154, 19);
            chkDesktop.TabIndex = 4;
            chkDesktop.Text = "Create Desktop Shortcut";
            // 
            // chkStartMenu
            // 
            chkStartMenu.Checked = true;
            chkStartMenu.CheckState = System.Windows.Forms.CheckState.Checked;
            chkStartMenu.Location = new System.Drawing.Point(146, 236);
            chkStartMenu.Name = "chkStartMenu";
            chkStartMenu.Size = new System.Drawing.Size(151, 19);
            chkStartMenu.TabIndex = 5;
            chkStartMenu.Text = "Create Start Menu Entry";
            // 
            // tabReady
            // 
            tabReady.BackColor = System.Drawing.Color.CornflowerBlue;
            tabReady.Controls.Add(pictureBox7);
            tabReady.Controls.Add(lblReadyTitle);
            tabReady.Controls.Add(lblReadyDesc);
            tabReady.ForeColor = System.Drawing.Color.FloralWhite;
            tabReady.Location = new System.Drawing.Point(4, 24);
            tabReady.Name = "tabReady";
            tabReady.Padding = new System.Windows.Forms.Padding(20);
            tabReady.Size = new System.Drawing.Size(592, 372);
            tabReady.TabIndex = 4;
            tabReady.Text = "5. Ready";
            // 
            // pictureBox7
            // 
            pictureBox7.Image = Properties.Resources.plumbob1;
            pictureBox7.Location = new System.Drawing.Point(8, 93);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new System.Drawing.Size(120, 180);
            pictureBox7.TabIndex = 11;
            pictureBox7.TabStop = false;
            // 
            // lblReadyTitle
            // 
            lblReadyTitle.AutoSize = true;
            lblReadyTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblReadyTitle.Location = new System.Drawing.Point(146, 20);
            lblReadyTitle.Name = "lblReadyTitle";
            lblReadyTitle.Size = new System.Drawing.Size(172, 30);
            lblReadyTitle.TabIndex = 0;
            lblReadyTitle.Text = "Ready to Install";
            // 
            // lblReadyDesc
            // 
            lblReadyDesc.Location = new System.Drawing.Point(146, 93);
            lblReadyDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblReadyDesc.Name = "lblReadyDesc";
            lblReadyDesc.Size = new System.Drawing.Size(410, 135);
            lblReadyDesc.TabIndex = 1;
            lblReadyDesc.Text = resources.GetString("lblReadyDesc.Text");
            // 
            // tabInstall
            // 
            tabInstall.BackColor = System.Drawing.Color.CornflowerBlue;
            tabInstall.Controls.Add(pictureBox9);
            tabInstall.Controls.Add(lblInstallTitle);
            tabInstall.Controls.Add(lblCurrentFile);
            tabInstall.Controls.Add(progCurrent);
            tabInstall.Controls.Add(lblOverall);
            tabInstall.Controls.Add(progOverall);
            tabInstall.ForeColor = System.Drawing.Color.FloralWhite;
            tabInstall.Location = new System.Drawing.Point(4, 24);
            tabInstall.Name = "tabInstall";
            tabInstall.Padding = new System.Windows.Forms.Padding(20);
            tabInstall.Size = new System.Drawing.Size(592, 372);
            tabInstall.TabIndex = 5;
            tabInstall.Text = "6. Install";
            // 
            // pictureBox9
            // 
            pictureBox9.Image = Properties.Resources.plumbob1;
            pictureBox9.Location = new System.Drawing.Point(8, 93);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new System.Drawing.Size(120, 180);
            pictureBox9.TabIndex = 11;
            pictureBox9.TabStop = false;
            // 
            // lblInstallTitle
            // 
            lblInstallTitle.AutoSize = true;
            lblInstallTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblInstallTitle.Location = new System.Drawing.Point(149, 20);
            lblInstallTitle.Name = "lblInstallTitle";
            lblInstallTitle.Size = new System.Drawing.Size(224, 30);
            lblInstallTitle.TabIndex = 0;
            lblInstallTitle.Text = "Installing SimTools...";
            // 
            // lblCurrentFile
            // 
            lblCurrentFile.AutoSize = true;
            lblCurrentFile.Location = new System.Drawing.Point(149, 93);
            lblCurrentFile.Name = "lblCurrentFile";
            lblCurrentFile.Size = new System.Drawing.Size(136, 15);
            lblCurrentFile.TabIndex = 1;
            lblCurrentFile.Text = "Preparing to download...";
            // 
            // progCurrent
            // 
            progCurrent.Location = new System.Drawing.Point(149, 118);
            progCurrent.Name = "progCurrent";
            progCurrent.Size = new System.Drawing.Size(411, 20);
            progCurrent.TabIndex = 2;
            // 
            // lblOverall
            // 
            lblOverall.AutoSize = true;
            lblOverall.Location = new System.Drawing.Point(149, 158);
            lblOverall.Name = "lblOverall";
            lblOverall.Size = new System.Drawing.Size(95, 15);
            lblOverall.TabIndex = 3;
            lblOverall.Text = "Overall Progress:";
            // 
            // progOverall
            // 
            progOverall.Location = new System.Drawing.Point(149, 183);
            progOverall.Name = "progOverall";
            progOverall.Size = new System.Drawing.Size(387, 25);
            progOverall.TabIndex = 4;
            // 
            // tabInterrupt
            // 
            tabInterrupt.BackColor = System.Drawing.Color.CornflowerBlue;
            tabInterrupt.Controls.Add(pictureBox11);
            tabInterrupt.Controls.Add(label2);
            tabInterrupt.Controls.Add(lblInterruptTitle);
            tabInterrupt.Controls.Add(lblInterruptDesc);
            tabInterrupt.ForeColor = System.Drawing.Color.FloralWhite;
            tabInterrupt.Location = new System.Drawing.Point(4, 24);
            tabInterrupt.Name = "tabInterrupt";
            tabInterrupt.Padding = new System.Windows.Forms.Padding(20);
            tabInterrupt.Size = new System.Drawing.Size(592, 372);
            tabInterrupt.TabIndex = 6;
            tabInterrupt.Text = "7. Interrupt";
            // 
            // pictureBox11
            // 
            pictureBox11.Image = Properties.Resources.plumbob1;
            pictureBox11.Location = new System.Drawing.Point(8, 93);
            pictureBox11.Name = "pictureBox11";
            pictureBox11.Size = new System.Drawing.Size(120, 180);
            pictureBox11.TabIndex = 13;
            pictureBox11.TabStop = false;
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(147, 142);
            label2.MaximumSize = new System.Drawing.Size(540, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(437, 65);
            label2.TabIndex = 2;
            label2.Text = resources.GetString("label2.Text");
            // 
            // lblInterruptTitle
            // 
            lblInterruptTitle.AutoSize = true;
            lblInterruptTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblInterruptTitle.ForeColor = System.Drawing.Color.Red;
            lblInterruptTitle.Location = new System.Drawing.Point(147, 20);
            lblInterruptTitle.Name = "lblInterruptTitle";
            lblInterruptTitle.Size = new System.Drawing.Size(250, 30);
            lblInterruptTitle.TabIndex = 0;
            lblInterruptTitle.Text = "Installation Interrupted";
            // 
            // lblInterruptDesc
            // 
            lblInterruptDesc.Location = new System.Drawing.Point(147, 93);
            lblInterruptDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblInterruptDesc.Name = "lblInterruptDesc";
            lblInterruptDesc.Size = new System.Drawing.Size(449, 33);
            lblInterruptDesc.TabIndex = 1;
            lblInterruptDesc.Text = "The installation was cancelled or encountered an error. Any changes made to your computer have been reverted.";
            // 
            // tabComplete
            // 
            tabComplete.BackColor = System.Drawing.Color.CornflowerBlue;
            tabComplete.Controls.Add(pictureBox13);
            tabComplete.Controls.Add(lblCompleteTitle);
            tabComplete.Controls.Add(lblCompleteDesc);
            tabComplete.Controls.Add(chkRun);
            tabComplete.ForeColor = System.Drawing.Color.FloralWhite;
            tabComplete.Location = new System.Drawing.Point(4, 24);
            tabComplete.Name = "tabComplete";
            tabComplete.Padding = new System.Windows.Forms.Padding(20);
            tabComplete.Size = new System.Drawing.Size(592, 372);
            tabComplete.TabIndex = 7;
            tabComplete.Text = "8. Complete";
            // 
            // pictureBox13
            // 
            pictureBox13.Image = Properties.Resources.plumbob1;
            pictureBox13.Location = new System.Drawing.Point(8, 93);
            pictureBox13.Name = "pictureBox13";
            pictureBox13.Size = new System.Drawing.Size(120, 180);
            pictureBox13.TabIndex = 15;
            pictureBox13.TabStop = false;
            // 
            // lblCompleteTitle
            // 
            lblCompleteTitle.AutoSize = true;
            lblCompleteTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblCompleteTitle.Location = new System.Drawing.Point(156, 20);
            lblCompleteTitle.Name = "lblCompleteTitle";
            lblCompleteTitle.Size = new System.Drawing.Size(235, 30);
            lblCompleteTitle.TabIndex = 0;
            lblCompleteTitle.Text = "Installation Complete";
            lblCompleteTitle.Click += lblCompleteTitle_Click;
            // 
            // lblCompleteDesc
            // 
            lblCompleteDesc.AutoSize = true;
            lblCompleteDesc.Location = new System.Drawing.Point(156, 93);
            lblCompleteDesc.MaximumSize = new System.Drawing.Size(540, 0);
            lblCompleteDesc.Name = "lblCompleteDesc";
            lblCompleteDesc.Size = new System.Drawing.Size(300, 15);
            lblCompleteDesc.TabIndex = 1;
            lblCompleteDesc.Text = "SimTools has been successfully installed on your system.";
            // 
            // chkRun
            // 
            chkRun.AutoSize = true;
            chkRun.Location = new System.Drawing.Point(156, 248);
            chkRun.Name = "chkRun";
            chkRun.Size = new System.Drawing.Size(121, 19);
            chkRun.TabIndex = 2;
            chkRun.Text = "Run SimTools now";
            // 
            // MainWindow
            // 
            ClientSize = new System.Drawing.Size(600, 450);
            Controls.Add(wizardTabs);
            Controls.Add(pnlBottom);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SimTools Installer";
            TransparencyKey = System.Drawing.Color.FromArgb(255, 128, 0);
            pnlBottom.ResumeLayout(false);
            wizardTabs.ResumeLayout(false);
            tabWelcome.ResumeLayout(false);
            tabWelcome.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PlumbobPictureBox).EndInit();
            tabLicense.ResumeLayout(false);
            tabLicense.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PlumbobPictureBox2).EndInit();
            tabArch.ResumeLayout(false);
            tabArch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            tabDirectory.ResumeLayout(false);
            tabDirectory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            tabReady.ResumeLayout(false);
            tabReady.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            tabInstall.ResumeLayout(false);
            tabInstall.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
            tabInterrupt.ResumeLayout(false);
            tabInterrupt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox11).EndInit();
            tabComplete.ResumeLayout(false);
            tabComplete.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox13).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnCancelExit;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.TabControl wizardTabs;
        private System.Windows.Forms.TabPage tabWelcome;
        private System.Windows.Forms.Label lblWelcomeTitle;
        private System.Windows.Forms.Label lblWelcomeDesc;
        private System.Windows.Forms.TabPage tabLicense;
        private System.Windows.Forms.Label lblLicenseTitle;
        private System.Windows.Forms.Label lblLicenseDesc;
        private System.Windows.Forms.TextBox txtLicense;
        private System.Windows.Forms.RadioButton radAgree;
        private System.Windows.Forms.RadioButton radDisagree;
        private System.Windows.Forms.TabPage tabArch;
        private System.Windows.Forms.Label lblArchTitle;
        private System.Windows.Forms.Label lblArchDesc;
        private System.Windows.Forms.RadioButton rad64Bit;
        private System.Windows.Forms.RadioButton rad32Bit;
        private System.Windows.Forms.TabPage tabDirectory;
        private System.Windows.Forms.Label lblDirTitle;
        private System.Windows.Forms.Label lblDirDesc;
        private System.Windows.Forms.TextBox txtInstallPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkDesktop;
        private System.Windows.Forms.CheckBox chkStartMenu;
        private System.Windows.Forms.TabPage tabReady;
        private System.Windows.Forms.Label lblReadyTitle;
        private System.Windows.Forms.Label lblReadyDesc;
        private System.Windows.Forms.TabPage tabInstall;
        private System.Windows.Forms.Label lblInstallTitle;
        private System.Windows.Forms.Label lblCurrentFile;
        private System.Windows.Forms.ProgressBar progCurrent;
        private System.Windows.Forms.Label lblOverall;
        private System.Windows.Forms.ProgressBar progOverall;
        private System.Windows.Forms.TabPage tabInterrupt;
        private System.Windows.Forms.Label lblInterruptTitle;
        private System.Windows.Forms.Label lblInterruptDesc;
        private System.Windows.Forms.TabPage tabComplete;
        private System.Windows.Forms.Label lblCompleteTitle;
        private System.Windows.Forms.Label lblCompleteDesc;
        private System.Windows.Forms.CheckBox chkRun;
        private System.Windows.Forms.PictureBox PlumbobPictureBox;
        private System.Windows.Forms.PictureBox PlumbobPictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.PictureBox pictureBox9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox11;
        private System.Windows.Forms.PictureBox pictureBox13;
    }
}