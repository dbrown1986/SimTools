using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SimToolsLanguageTool
{
    public partial class MainForm : Form
    {
        private LangFile sourceLang = new LangFile();
        private LangFile targetLang = new LangFile();

        // Tracks target textboxes for Save operations ("SectionName|KeyName" -> TextBox)
        private Dictionary<string, TextBox> targetControls = new Dictionary<string, TextBox>();

        // Tracks state of rows that are missing translations for theme coloring
        private Dictionary<TextBox, bool> missingStatusMap = new Dictionary<TextBox, bool>();

        // Tracks baseline snapshots for change verification
        private Dictionary<string, string> originalSavedValues = new Dictionary<string, string>();

        // High-Performance Layout Trackers (maps SectionName to its corresponding components)
        private Dictionary<string, Label> sectionHeaderLabels = new Dictionary<string, Label>();
        private Dictionary<string, Button> sectionAddButtons = new Dictionary<string, Button>();
        private Dictionary<string, Panel> sectionHeaderPanels = new Dictionary<string, Panel>();
        private Dictionary<string, Label> sourceHeaderLabels = new Dictionary<string, Label>();

        // Dynamic Color Table Customization to prevent generic GDI+ background painting crashes
        private CustomColorTable customColorTable = new CustomColorTable();

        // Reverse lookups to map a control back to its composite LangFile coordinates for deletion/editing updates
        private Dictionary<Control, (string Section, string Key)> controlMetaDataMap = new Dictionary<Control, (string Section, string Key)>();
        // Tracks pairs of controls per row so we can clean up both columns simultaneously upon removal
        private Dictionary<Control, List<Control>> rowControlGroups = new Dictionary<Control, List<Control>>();

        // State Tracking
        private bool isDarkMode = false;

        // UI Controls
        private ToolStrip toolStrip = new ToolStrip();
        private ToolStripButton? btnThemeToggle;
        private Panel panelSourceScroll = new Panel();
        private Panel panelTargetScroll = new Panel();
        private TableLayoutPanel tlpSource = new TableLayoutPanel();
        private TableLayoutPanel tlpTarget = new TableLayoutPanel();
        private Label lblSourceTitle = new Label();
        private Label lblTargetTitle = new Label();
        private StatusStrip statusStrip = new StatusStrip();
        private ToolStripStatusLabel lblSourcePath = new ToolStripStatusLabel();
        private ToolStripStatusLabel lblTargetPath = new ToolStripStatusLabel();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED: Paints descendants efficiently to avoid flashing
                return cp;
            }
        }

        public MainForm()
        {
            // Automatically extracts the icon embedded in your project's compiled assembly output
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // Fallback gracefully if execution environment restricts access
            }

            InitializeCustomLayout();
            DetectSystemTheme();
            ApplyTheme();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private void InitializeCustomLayout()
        {
            this.Text = "SimTools Language Manager";
            this.Width = 1300; // Expanded width to support the new button real estate safely
            this.Height = 800;
            this.MinimumSize = new Size(900, 500);

            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Padding = new Padding(10, 5, 10, 5);
            toolStrip.Renderer = new ToolStripProfessionalRenderer(customColorTable);

            ToolStripButton btnLoadSource = new ToolStripButton("📂 Load Source (Base)", null, LoadSource_Click) { Padding = new Padding(5) };
            ToolStripButton btnNewTarget = new ToolStripButton("🆕 New Target (Copy Source)", null, NewTarget_Click) { Padding = new Padding(5) };
            ToolStripButton btnLoadTarget = new ToolStripButton("📥 Load Target (Edit)", null, LoadTarget_Click) { Padding = new Padding(5) };
            ToolStripButton btnSaveTarget = new ToolStripButton("💾 Save Target", null, SaveTarget_Click) { Padding = new Padding(5) };
            ToolStripButton btnAddEntry = new ToolStripButton("➕ Add Custom Entry", null, AddEntry_Click) { Padding = new Padding(5) };

            ToolStripButton btnHelpInstructions = new ToolStripButton("❓ How to use this tool", null, HelpInstructions_Click) { Padding = new Padding(5), Alignment = ToolStripItemAlignment.Right };
            btnThemeToggle = new ToolStripButton("🌙 Dark Mode", null, ToggleTheme_Click) { Padding = new Padding(5), Alignment = ToolStripItemAlignment.Right };

            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnLoadSource, new ToolStripSeparator(),
                btnNewTarget, btnLoadTarget, btnSaveTarget, new ToolStripSeparator(),
                btnAddEntry, btnThemeToggle, btnHelpInstructions
            });
            this.Controls.Add(toolStrip);

            lblSourcePath.Text = "Source: None loaded";
            lblSourcePath.Padding = new Padding(0, 0, 20, 0);
            lblTargetPath.Text = "Target: None loaded";
            statusStrip.Items.AddRange(new ToolStripItem[] { lblSourcePath, lblTargetPath });
            this.Controls.Add(statusStrip);

            SplitContainer splitter = new SplitContainer();
            splitter.Dock = DockStyle.Fill;
            splitter.SplitterWidth = 6;
            this.Controls.Add(splitter);
            splitter.BringToFront();

            this.PerformLayout();
            splitter.SplitterDistance = this.ClientSize.Width / 2;

            // --- LEFT PANEL ---
            Panel leftContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            lblSourceTitle.Text = "SOURCE BASE LANGUAGE (Read-Only)";
            lblSourceTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSourceTitle.Dock = DockStyle.Top;
            lblSourceTitle.Height = 30;

            panelSourceScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
            tlpSource = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };
            tlpSource.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpSource.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            typeof(TableLayoutPanel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(tlpSource, true, null);

            panelSourceScroll.Controls.Add(tlpSource);
            leftContainer.Controls.Add(panelSourceScroll);
            leftContainer.Controls.Add(lblSourceTitle);
            splitter.Panel1.Controls.Add(leftContainer);

            // --- RIGHT PANEL ---
            Panel rightContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            lblTargetTitle.Text = "TARGET TRANSLATION (Editable Workspace)";
            lblTargetTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTargetTitle.Dock = DockStyle.Top;
            lblTargetTitle.Height = 30;

            panelTargetScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BorderStyle = BorderStyle.FixedSingle };
            tlpTarget = new TableLayoutPanel { ColumnCount = 3, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };
            tlpTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tlpTarget.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45F));
            typeof(TableLayoutPanel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(tlpTarget, true, null);

            panelTargetScroll.Controls.Add(tlpTarget);
            rightContainer.Controls.Add(panelTargetScroll);
            rightContainer.Controls.Add(lblTargetTitle);
            splitter.Panel2.Controls.Add(rightContainer);
        }

        private void DetectSystemTheme()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue("AppsUseLightTheme");
                        if (value != null) isDarkMode = (int)value == 0;
                    }
                }
            }
            catch { isDarkMode = false; }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                DetectSystemTheme();
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            Color backColor = isDarkMode ? Color.FromArgb(28, 28, 28) : SystemColors.Control;
            Color panelBg = isDarkMode ? Color.FromArgb(35, 35, 38) : Color.White;
            Color textColor = isDarkMode ? Color.White : Color.Black;
            Color headerBg = isDarkMode ? Color.FromArgb(51, 51, 55) : Color.FromArgb(230, 235, 240);
            Color readOnlyBoxBg = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245);
            Color editBoxBg = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            Color missingBg = isDarkMode ? Color.FromArgb(74, 35, 40) : Color.MistyRose;
            Color toolStripBg = isDarkMode ? Color.FromArgb(45, 45, 48) : SystemColors.Control;

            this.BackColor = backColor;

            customColorTable.ThemeBackgroundColor = toolStripBg;
            toolStrip.BackColor = toolStripBg;
            toolStrip.ForeColor = textColor;
            statusStrip.BackColor = toolStripBg;
            statusStrip.ForeColor = textColor;

            if (btnThemeToggle != null) btnThemeToggle.Text = isDarkMode ? "☀️ Light Mode" : "🌙 Dark Mode";

            lblSourceTitle.ForeColor = isDarkMode ? Color.DarkGray : Color.DimGray;
            lblTargetTitle.ForeColor = isDarkMode ? Color.DeepSkyBlue : Color.SteelBlue;

            panelSourceScroll.BackColor = panelBg;
            panelTargetScroll.BackColor = panelBg;
            tlpSource.BackColor = panelBg;
            tlpTarget.BackColor = panelBg;

            tlpSource.SuspendLayout();
            tlpTarget.SuspendLayout();

            foreach (Control ctrl in tlpSource.Controls)
            {
                if (ctrl is Label lbl)
                {
                    lbl.ForeColor = textColor;
                    if (lbl.Tag?.ToString() == "Header") lbl.BackColor = headerBg;
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = readOnlyBoxBg;
                    txt.ForeColor = isDarkMode ? Color.LightGray : Color.DarkSlateGray;
                }
            }

            foreach (Control ctrl in tlpTarget.Controls)
            {
                if (ctrl is Label lbl)
                {
                    lbl.ForeColor = textColor;
                    if (lbl.Tag?.ToString() == "Header") lbl.BackColor = headerBg;
                }
                else if (ctrl is Panel pnl && pnl.Tag?.ToString() == "HeaderPanel")
                {
                    pnl.BackColor = headerBg;
                    foreach (Control child in pnl.Controls)
                    {
                        child.ForeColor = textColor;
                        if (child is Button btn)
                        {
                            btn.BackColor = isDarkMode ? Color.FromArgb(70, 70, 75) : Color.FromArgb(210, 215, 220);
                            btn.ForeColor = textColor;
                        }
                    }
                }
                else if (ctrl is TextBox txt)
                {
                    txt.ForeColor = textColor;
                    txt.BackColor = (missingStatusMap.ContainsKey(txt) && missingStatusMap[txt]) ? missingBg : editBoxBg;
                }
                else if (ctrl is Button btn && btn.Tag?.ToString() == "RemoveAction")
                {
                    btn.BackColor = isDarkMode ? Color.FromArgb(60, 30, 30) : Color.FromArgb(255, 230, 230);
                    btn.ForeColor = isDarkMode ? Color.FromArgb(255, 120, 120) : Color.DarkRed;
                }
            }

            tlpSource.ResumeLayout();
            tlpTarget.ResumeLayout();
        }

        private void PopulateFullUI()
        {
            tlpSource.SuspendLayout();
            tlpTarget.SuspendLayout();

            tlpSource.Controls.Clear();
            tlpSource.RowStyles.Clear();
            tlpTarget.Controls.Clear();
            tlpTarget.RowStyles.Clear();

            targetControls.Clear();
            missingStatusMap.Clear();
            sectionHeaderLabels.Clear();
            sectionAddButtons.Clear();
            sectionHeaderPanels.Clear();
            sourceHeaderLabels.Clear();
            controlMetaDataMap.Clear();
            rowControlGroups.Clear();

            tlpSource.RowCount = 0;
            tlpTarget.RowCount = 0;

            foreach (var section in sourceLang.Data)
            {
                AddSectionHeaderUI(section.Key);

                foreach (var kvp in section.Value)
                {
                    AddRowUI(section.Key, kvp.Key, kvp.Value,
                        (targetLang.Data.TryGetValue(section.Key, out var sub) && sub.TryGetValue(kvp.Key, out var val)) ? val : "",
                        sub != null && sub.ContainsKey(kvp.Key));
                }
            }

            CaptureBaselineSnapshot();
            tlpSource.ResumeLayout();
            tlpTarget.ResumeLayout();
            ApplyTheme();
        }

        private void AddSectionHeaderUI(string sectionName)
        {
            Font headerFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            Padding headerMargin = new Padding(0, 15, 0, 5);

            tlpSource.RowCount++;
            tlpSource.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label lblSrcHeader = new Label { Text = $"  [{sectionName}]", Font = headerFont, Margin = headerMargin, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Tag = "Header", Anchor = AnchorStyles.Left | AnchorStyles.Right };
            tlpSource.Controls.Add(lblSrcHeader, 0, tlpSource.RowCount - 1);
            tlpSource.SetColumnSpan(lblSrcHeader, 2);
            sourceHeaderLabels[sectionName] = lblSrcHeader;

            tlpTarget.RowCount++;
            tlpTarget.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Panel headerPanel = new Panel { Height = 30, Margin = headerMargin, Tag = "HeaderPanel", Dock = DockStyle.Fill };
            Label lblTgtHeader = new Label { Text = $"  [{sectionName}]", Font = headerFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Tag = sectionName };
            lblTgtHeader.DoubleClick += SectionHeader_DoubleClick;

            Button btnAddRow = new Button { Text = "＋", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Width = 30, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, Tag = sectionName };
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.Click += AddSectionInline_Click;

            headerPanel.Controls.Add(lblTgtHeader);
            headerPanel.Controls.Add(btnAddRow);

            sectionHeaderLabels[sectionName] = lblTgtHeader;
            sectionAddButtons[sectionName] = btnAddRow;
            sectionHeaderPanels[sectionName] = headerPanel;

            tlpTarget.Controls.Add(headerPanel, 0, tlpTarget.RowCount - 1);
            tlpTarget.SetColumnSpan(headerPanel, 3);
        }

        private void AddRowUI(string section, string key, string srcVal, string tgtVal, bool existsInTarget)
        {
            Font textFont = new Font("Segoe UI", 9F);
            Padding cellMargin = new Padding(4);

            tlpSource.RowCount++;
            tlpSource.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label lblSrcKey = new Label { Text = key, Font = textFont, Margin = cellMargin, Anchor = AnchorStyles.Left | AnchorStyles.Right, AutoSize = true, UseMnemonic = false };
            TextBox txtSrcVal = new TextBox { Text = srcVal, Font = textFont, Margin = cellMargin, ReadOnly = true, Multiline = true, WordWrap = true, Height = 28, Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };
            tlpSource.Controls.Add(lblSrcKey, 0, tlpSource.RowCount - 1);
            tlpSource.Controls.Add(txtSrcVal, 1, tlpSource.RowCount - 1);

            tlpTarget.RowCount++;
            tlpTarget.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label lblTgtKey = new Label { Text = key, Font = textFont, Margin = cellMargin, Anchor = AnchorStyles.Left | AnchorStyles.Right, AutoSize = true, UseMnemonic = false };

            lblTgtKey.DoubleClick += KeyLabel_DoubleClick;
            lblTgtKey.Cursor = Cursors.Hand;
            ToolTip tt = new ToolTip();
            tt.SetToolTip(lblTgtKey, "Double-click to rename this Key identifier");

            TextBox txtTgtVal = new TextBox { Text = tgtVal, Font = textFont, Margin = cellMargin, Multiline = true, WordWrap = true, Height = 28, Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };

            Button btnRemove = new Button { Text = "❌", Font = new Font("Segoe UI", 8F), Margin = cellMargin, Width = 35, Height = 26, FlatStyle = FlatStyle.Flat, Tag = "RemoveAction" };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += RemoveRow_Click;

            missingStatusMap.Add(txtTgtVal, !existsInTarget);
            targetControls.Add($"{section}|{key}", txtTgtVal);

            controlMetaDataMap[lblTgtKey] = (section, key);
            controlMetaDataMap[txtTgtVal] = (section, key);
            controlMetaDataMap[btnRemove] = (section, key);

            var leftControls = new List<Control> { lblSrcKey, txtSrcVal };
            var rightControls = new List<Control> { lblTgtKey, txtTgtVal, btnRemove };

            rowControlGroups[lblTgtKey] = rightControls;
            rowControlGroups[txtTgtVal] = rightControls;
            rowControlGroups[btnRemove] = rightControls;
            rowControlGroups[lblSrcKey] = leftControls;
            rowControlGroups[txtSrcVal] = leftControls;

            tlpTarget.Controls.Add(lblTgtKey, 0, tlpTarget.RowCount - 1);
            tlpTarget.Controls.Add(txtTgtVal, 1, tlpTarget.RowCount - 1);
            tlpTarget.Controls.Add(btnRemove, 2, tlpTarget.RowCount - 1);
        }

        private void InsertRowAtTargetSection(TableLayoutPanel tlp, List<Control> controlsToInsert, Control targetHeaderControl)
        {
            int targetRow = tlp.GetRow(targetHeaderControl) + 1;

            for (int r = targetRow; r < tlp.RowCount; r++)
            {
                Control? c0 = tlp.GetControlFromPosition(0, r);
                if (c0 != null && (c0.Tag?.ToString() == "Header" || c0.Tag?.ToString() == "HeaderPanel"))
                {
                    targetRow = r;
                    break;
                }
                if (r == tlp.RowCount - 1)
                {
                    targetRow = tlp.RowCount;
                }
            }

            tlp.RowCount++;
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int r = tlp.RowCount - 2; r >= targetRow; r--)
            {
                for (int col = 0; col < tlp.ColumnCount; col++)
                {
                    Control? ctrl = tlp.GetControlFromPosition(col, r);
                    if (ctrl != null) tlp.SetRow(ctrl, r + 1);
                }
            }

            for (int i = 0; i < controlsToInsert.Count; i++)
            {
                tlp.Controls.Add(controlsToInsert[i], i, targetRow);
            }
        }

        private void RemoveRow_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && controlMetaDataMap.TryGetValue(btn, out var meta))
            {
                DialogResult confirmation = MessageBox.Show(
                    $"Are you sure you want to permanently remove the key entry '{meta.Key}' from the [{meta.Section}] section?",
                    "Confirm Deletion Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmation == DialogResult.No) return;

                tlpSource.SuspendLayout();
                tlpTarget.SuspendLayout();

                if (sourceLang.Data.TryGetValue(meta.Section, out var srcSub)) srcSub.Remove(meta.Key);
                if (targetLang.Data.TryGetValue(meta.Section, out var tgtSub)) tgtSub.Remove(meta.Key);

                string targetControlKey = $"{meta.Section}|{meta.Key}";
                if (targetControls.TryGetValue(targetControlKey, out var txtBox)) missingStatusMap.Remove(txtBox);
                targetControls.Remove(targetControlKey);

                PurgeControlGroupFromLayout(tlpTarget, btn);

                Control? standardSourceKeyControl = null;
                foreach (Control c in tlpSource.Controls)
                {
                    if (c is Label lbl && lbl.Text == meta.Key)
                    {
                        int row = tlpSource.GetRow(lbl);
                        Control? precedingHeader = FindPrecedingHeaderControl(tlpSource, row);
                        if (precedingHeader != null && precedingHeader.Tag?.ToString() == "Header" && precedingHeader.Text.Contains($"[{meta.Section}]"))
                        {
                            standardSourceKeyControl = lbl;
                            break;
                        }
                    }
                }

                if (standardSourceKeyControl != null)
                {
                    PurgeControlGroupFromLayout(tlpSource, standardSourceKeyControl);
                }

                tlpSource.ResumeLayout(true);
                tlpTarget.ResumeLayout(true);
            }
        }

        private void PurgeControlGroupFromLayout(TableLayoutPanel tlp, Control structuralControl)
        {
            int rowToDelete = tlp.GetRow(structuralControl);
            if (rowToDelete == -1) return;

            if (rowControlGroups.TryGetValue(structuralControl, out var group))
            {
                foreach (Control c in group)
                {
                    tlp.Controls.Remove(c);
                    controlMetaDataMap.Remove(c);
                    rowControlGroups.Remove(c);
                    c.Dispose();
                }
            }

            for (int r = rowToDelete + 1; r < tlp.RowCount; r++)
            {
                for (int col = 0; col < tlp.ColumnCount; col++)
                {
                    Control? ctrl = tlp.GetControlFromPosition(col, r);
                    if (ctrl != null) tlp.SetRow(ctrl, r - 1);
                }
            }

            if (tlp.RowCount > 0)
            {
                tlp.RowCount--;
                if (tlp.RowStyles.Count > tlp.RowCount) tlp.RowStyles.RemoveAt(tlp.RowCount);
            }
        }

        private Control? FindPrecedingHeaderControl(TableLayoutPanel tlp, int startRow)
        {
            for (int r = startRow - 1; r >= 0; r--)
            {
                Control? c = tlp.GetControlFromPosition(0, r);
                if (c != null && (c.Tag?.ToString() == "Header" || c.Tag?.ToString() == "HeaderPanel")) return c;
            }
            return null;
        }

        private void KeyLabel_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is Label lbl && controlMetaDataMap.TryGetValue(lbl, out var meta))
            {
                using (Form prompt = new Form { Width = 360, Height = 180, Text = "Rename Key Identifier", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
                {
                    prompt.BackColor = this.BackColor;
                    prompt.ForeColor = isDarkMode ? Color.White : Color.Black;

                    Label lblInfo = new Label { Left = 20, Top = 20, Width = 300, Text = $"Enter new key value name inside [{meta.Section}]:", Font = new Font("Segoe UI", 9) };
                    TextBox txtNewName = new TextBox { Left = 20, Top = 50, Width = 300, Text = meta.Key, Font = new Font("Segoe UI", 9) };
                    Button btnConfirm = new Button { Text = "Apply Key Change", Left = 180, Top = 90, Width = 140, Height = 30, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.System };

                    prompt.Controls.AddRange(new Control[] { lblInfo, txtNewName, btnConfirm });
                    prompt.AcceptButton = btnConfirm;

                    if (prompt.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtNewName.Text))
                    {
                        string newKeyName = txtNewName.Text.Trim();
                        if (string.Equals(meta.Key, newKeyName, StringComparison.Ordinal)) return;

                        if (sourceLang.Data.TryGetValue(meta.Section, out var srcSub) && srcSub.ContainsKey(meta.Key))
                        {
                            string val = srcSub[meta.Key];
                            srcSub.Remove(meta.Key);
                            srcSub[newKeyName] = val;
                        }
                        if (targetLang.Data.TryGetValue(meta.Section, out var tgtSub) && tgtSub.ContainsKey(meta.Key))
                        {
                            string val = tgtSub[meta.Key];
                            tgtSub.Remove(meta.Key);
                            tgtSub[newKeyName] = val;
                        }

                        string oldMapKey = $"{meta.Section}|{meta.Key}";
                        string newMapKey = $"{meta.Section}|{newKeyName}";
                        if (targetControls.TryGetValue(oldMapKey, out var associatedTxtBox))
                        {
                            targetControls.Remove(oldMapKey);
                            targetControls[newMapKey] = associatedTxtBox;
                        }

                        lbl.Text = newKeyName;

                        int targetRow = tlpTarget.GetRow(lbl);
                        if (targetRow != -1)
                        {
                            Control? srcLabel = tlpSource.GetControlFromPosition(0, targetRow);
                            if (srcLabel is Label sLabel) sLabel.Text = newKeyName;
                        }

                        if (rowControlGroups.TryGetValue(lbl, out var group))
                        {
                            foreach (Control c in group)
                            {
                                controlMetaDataMap[c] = (meta.Section, newKeyName);
                            }
                        }
                    }
                }
            }
        }

        private void AddSectionInline_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                PromptAndAddEntry(btn.Tag.ToString()!);
            }
        }

        private void SectionHeader_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.Tag != null)
            {
                string oldSectionName = lbl.Tag.ToString()!;

                using (Form prompt = new Form { Width = 340, Height = 180, Text = "Rename Section Container", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
                {
                    prompt.BackColor = this.BackColor;
                    prompt.ForeColor = isDarkMode ? Color.White : Color.Black;

                    Label lblInfo = new Label { Left = 20, Top = 20, Width = 280, Text = $"Enter new title for [{oldSectionName}]:", Font = new Font("Segoe UI", 9) };
                    TextBox txtNewName = new TextBox { Left = 20, Top = 50, Width = 280, Text = oldSectionName, Font = new Font("Segoe UI", 9) };
                    Button btnConfirm = new Button { Text = "Apply Change", Left = 180, Top = 90, Width = 120, Height = 30, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.System };

                    prompt.Controls.AddRange(new Control[] { lblInfo, txtNewName, btnConfirm });
                    prompt.AcceptButton = btnConfirm;

                    if (prompt.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtNewName.Text))
                    {
                        string newSectionName = txtNewName.Text.Trim();
                        if (string.Equals(oldSectionName, newSectionName, StringComparison.OrdinalIgnoreCase)) return;

                        if (sourceLang.Data.ContainsKey(oldSectionName))
                        {
                            var contents = sourceLang.Data[oldSectionName];
                            sourceLang.Data.Remove(oldSectionName);
                            sourceLang.Data[newSectionName] = contents;
                        }
                        if (targetLang.Data.ContainsKey(oldSectionName))
                        {
                            var contents = targetLang.Data[oldSectionName];
                            targetLang.Data.Remove(oldSectionName);
                            targetLang.Data[newSectionName] = contents;
                        }

                        if (sectionHeaderLabels.TryGetValue(oldSectionName, out var headerLabel))
                        {
                            headerLabel.Text = $"  [{newSectionName}]";
                            headerLabel.Tag = newSectionName;
                            sectionHeaderLabels.Remove(oldSectionName);
                            sectionHeaderLabels[newSectionName] = headerLabel;
                        }
                        if (sourceHeaderLabels.TryGetValue(oldSectionName, out var srcHeaderLabel))
                        {
                            srcHeaderLabel.Text = $"  [{newSectionName}]";
                            sourceHeaderLabels.Remove(oldSectionName);
                            sourceHeaderLabels[newSectionName] = srcHeaderLabel;
                        }
                        if (sectionAddButtons.TryGetValue(oldSectionName, out var addBtn))
                        {
                            addBtn.Tag = newSectionName;
                            sectionAddButtons.Remove(oldSectionName);
                            sectionAddButtons[newSectionName] = addBtn;
                        }
                        if (sectionHeaderPanels.TryGetValue(oldSectionName, out var panel))
                        {
                            sectionHeaderPanels.Remove(oldSectionName);
                            sectionHeaderPanels[newSectionName] = panel;
                        }

                        var updatedTargetControls = new Dictionary<string, TextBox>();
                        foreach (var kvp in targetControls)
                        {
                            string[] parts = kvp.Key.Split('|');
                            string currentSec = parts[0] == oldSectionName ? newSectionName : parts[0];
                            updatedTargetControls[$"{currentSec}|{parts[1]}"] = kvp.Value;
                        }
                        targetControls = updatedTargetControls;

                        foreach (var item in controlMetaDataMap.Keys)
                        {
                            var metaVal = controlMetaDataMap[item];
                            if (metaVal.Section == oldSectionName)
                            {
                                controlMetaDataMap[item] = (newSectionName, metaVal.Key);
                            }
                        }
                    }
                }
            }
        }

        private void PromptAndAddEntry(string prefilledSection)
        {
            using (Form prompt = new Form { Width = 340, Height = 240, Text = "Add New Entry Definition", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
            {
                prompt.BackColor = this.BackColor;
                prompt.ForeColor = isDarkMode ? Color.White : Color.Black;

                Label lblSec = new Label() { Left = 20, Top = 20, Text = "Section:", Font = new Font("Segoe UI", 9) };
                TextBox txtSec = new TextBox() { Left = 110, Top = 18, Width = 180, Text = prefilledSection, Font = new Font("Segoe UI", 9) };
                Label lblKey = new Label() { Left = 20, Top = 60, Text = "Key Name:", Font = new Font("Segoe UI", 9) };
                TextBox txtKey = new TextBox() { Left = 110, Top = 58, Width = 180, Font = new Font("Segoe UI", 9) };
                Button confirmation = new Button() { Text = "Inject", Left = 190, Width = 100, Top = 120, Height = 30, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.System };

                prompt.Controls.AddRange(new Control[] { lblSec, txtSec, lblKey, txtKey, confirmation });
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtKey.Text))
                {
                    string section = txtSec.Text.Trim();
                    string key = txtKey.Text.Trim();

                    if (!sourceLang.Data.ContainsKey(section))
                    {
                        sourceLang.Data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        sourceLang.Data[section][key] = "[New Entry]";
                        PopulateFullUI();
                        return;
                    }

                    sourceLang.Data[section][key] = "[New Entry]";

                    tlpSource.SuspendLayout();
                    tlpTarget.SuspendLayout();

                    Font textFont = new Font("Segoe UI", 9F);
                    Padding cellMargin = new Padding(4);

                    Label lblSrcKey = new Label { Text = key, Font = textFont, Margin = cellMargin, Anchor = AnchorStyles.Left | AnchorStyles.Right, AutoSize = true, UseMnemonic = false };
                    TextBox txtSrcVal = new TextBox { Text = "[New Entry]", Font = textFont, Margin = cellMargin, ReadOnly = true, Multiline = true, WordWrap = true, Height = 28, Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };

                    Label lblTgtKey = new Label { Text = key, Font = textFont, Margin = cellMargin, Anchor = AnchorStyles.Left | AnchorStyles.Right, AutoSize = true, UseMnemonic = false, Cursor = Cursors.Hand };
                    lblTgtKey.DoubleClick += KeyLabel_DoubleClick;
                    new ToolTip().SetToolTip(lblTgtKey, "Double-click to rename this Key identifier");

                    TextBox txtTgtVal = new TextBox { Text = "", Font = textFont, Margin = cellMargin, Multiline = true, WordWrap = true, Height = 28, Anchor = AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.FixedSingle };

                    Button btnRemove = new Button { Text = "❌", Font = new Font("Segoe UI", 8F), Margin = cellMargin, Width = 35, Height = 26, FlatStyle = FlatStyle.Flat, Tag = "RemoveAction" };
                    btnRemove.FlatAppearance.BorderSize = 0;
                    btnRemove.Click += RemoveRow_Click;

                    missingStatusMap.Add(txtTgtVal, true);
                    targetControls.Add($"{section}|{key}", txtTgtVal);

                    controlMetaDataMap[lblTgtKey] = (section, key);
                    controlMetaDataMap[txtTgtVal] = (section, key);
                    controlMetaDataMap[btnRemove] = (section, key);

                    var leftGroup = new List<Control> { lblSrcKey, txtSrcVal };
                    var rightGroup = new List<Control> { lblTgtKey, txtTgtVal, btnRemove };

                    rowControlGroups[lblTgtKey] = rightGroup;
                    rowControlGroups[txtTgtVal] = rightGroup;
                    rowControlGroups[btnRemove] = rightGroup;
                    rowControlGroups[lblSrcKey] = leftGroup;
                    rowControlGroups[txtSrcVal] = leftGroup;

                    if (sourceHeaderLabels.TryGetValue(section, out var srcHeader) && sectionHeaderPanels.TryGetValue(section, out var tgtHeader))
                    {
                        InsertRowAtTargetSection(tlpSource, leftGroup, srcHeader);
                        InsertRowAtTargetSection(tlpTarget, rightGroup, tgtHeader);
                    }

                    tlpSource.ResumeLayout(true);
                    tlpTarget.ResumeLayout(true);

                    Color textCol = isDarkMode ? Color.White : Color.Black;
                    lblSrcKey.ForeColor = textCol;
                    txtSrcVal.ForeColor = isDarkMode ? Color.LightGray : Color.DarkSlateGray;
                    txtSrcVal.BackColor = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(245, 245, 245);

                    lblTgtKey.ForeColor = textCol;
                    txtTgtVal.ForeColor = textCol;
                    txtTgtVal.BackColor = isDarkMode ? Color.FromArgb(74, 35, 40) : Color.MistyRose;

                    btnRemove.BackColor = isDarkMode ? Color.FromArgb(60, 30, 30) : Color.FromArgb(255, 230, 230);
                    btnRemove.ForeColor = isDarkMode ? Color.FromArgb(255, 120, 120) : Color.DarkRed;
                }
            }
        }

        private void CaptureBaselineSnapshot()
        {
            originalSavedValues.Clear();
            foreach (var item in targetControls) originalSavedValues[item.Key] = item.Value.Text;
        }

        private bool UnsavedChangesExist()
        {
            if (targetControls.Count != originalSavedValues.Count) return true;
            foreach (var item in targetControls)
            {
                if (!originalSavedValues.TryGetValue(item.Key, out var originalText) || originalText != item.Value.Text)
                    return true;
            }
            return false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (UnsavedChangesExist())
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved workspace translation modifications. Do you want to exit without saving?",
                    "Unsaved Changes Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            base.OnFormClosing(e);
        }

        private void ToggleTheme_Click(object? sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        private void HelpInstructions_Click(object? sender, EventArgs e)
        {
            TaskDialogPage page = new TaskDialogPage()
            {
                Heading = "SimTools Language Manager Quick Guide",
                Text = "Follow these basic steps to synchronize and edit your translation files efficiently:",
                Icon = TaskDialogIcon.Information,
                Caption = "How to Use This Tool",
                Buttons = { TaskDialogButton.OK },
                Footnote = new TaskDialogFootnote() { Text = "Double-click key names or section banners to rename them instantly." },
                Expander = new TaskDialogExpander()
                {
                    Text = "Detailed Workspace Operations:\n\n" +
                           "1. Load Source (Base): Opens your authoritative baseline localization file (*.lang). This acts as your read-only translation reference.\n\n" +
                           "2. New Target (Copy Source): Clones the loaded baseline structure directly into your working target layout so you can start a new translation file entirely from scratch.\n\n" +
                           "3. Load Target (Edit): Opens an existing target file. Missing translations will instantly highlight in soft red.\n\n" +
                           "4. Adding Entries: Use the global layout button or click '＋' on any section header banner to inline inject a localized string property precisely where it belongs.\n\n" +
                           "5. Management Action Items: Click '❌' next to any entry to remove it entirely from active file configurations, or double-click key labels to update their identifiers inline.\n\n" +
                           "⚠️ File Naming Rule:\n" +
                           "When saving your final translation file, you must name it using the standard 2-letter country code of the target language (e.g., fr.lang for French, es.lang for Spanish, de.lang for German, ja.lang for Japanese, etc.) when it is later added to SimTools, this is how the language manager in SimTools will recognize and load it.\n \n" +
                           "PLEASE NOTE: Language files do not come bundled with the Language Tool. You will need to load them in from the SimTools/Languages directory.",
                    Expanded = true
                }
            };

            TaskDialog.ShowDialog(this, page);
        }

        private void LoadSource_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Language Files (*.lang)|*.lang|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    sourceLang.Load(ofd.FileName);
                    lblSourcePath.Text = $"Source: {Path.GetFileName(ofd.FileName)}";
                    PopulateFullUI();
                }
            }
        }

        // Deep Copies Source Data Structures to Instantiate a Brand New Target Layout File Container
        private void NewTarget_Click(object? sender, EventArgs e)
        {
            if (sourceLang.Data.Count == 0)
            {
                MessageBox.Show("Please load a Source (Base) file first to use as a template structure for your new translation.",
                    "No Source Data Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UnsavedChangesExist())
            {
                DialogResult confirmOverwite = MessageBox.Show(
                    "You have unsaved changes in your current target workspace. Creating a new target file will discard them. Proceed?",
                    "Unsaved Changes Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmOverwite == DialogResult.No) return;
            }

            targetLang.Data.Clear();

            // Perform deep mirror synchronization pass across internal sections dictionary objects
            foreach (var sectionKvp in sourceLang.Data)
            {
                var replicatedSection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var innerKvp in sectionKvp.Value)
                {
                    // Copy keys but leave the values blank (or match source baseline values if you prefer pre-filled data templates)
                    replicatedSection[innerKvp.Key] = innerKvp.Value;
                }
                targetLang.Data[sectionKvp.Key] = replicatedSection;
            }

            lblTargetPath.Text = "Target: [New Unsaved Translation Template File]";
            PopulateFullUI();
        }

        private void LoadTarget_Click(object? sender, EventArgs e)
        {
            if (UnsavedChangesExist())
            {
                DialogResult confirmOverwrite = MessageBox.Show(
                    "You have unsaved changes in your current workspace. Loading a new target will discard them. Proceed?",
                    "Unsaved Changes Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmOverwrite == DialogResult.No) return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Language Files (*.lang)|*.lang|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetLang.Load(ofd.FileName);
                    lblTargetPath.Text = $"Target: {Path.GetFileName(ofd.FileName)}";
                    PopulateFullUI();
                }
            }
        }

        private void SaveTarget_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Language Files (*.lang)|*.lang" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var controlKvp in targetControls)
                    {
                        string[] parts = controlKvp.Key.Split('|');
                        if (!targetLang.Data.ContainsKey(parts[0]))
                            targetLang.Data[parts[0]] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        targetLang.Data[parts[0]][parts[1]] = controlKvp.Value.Text;
                    }

                    targetLang.Save(sfd.FileName);
                    lblTargetPath.Text = $"Target: {Path.GetFileName(sfd.FileName)}"; // Dynamic label update upon creation save completion
                    CaptureBaselineSnapshot();
                    MessageBox.Show("Target file saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void AddEntry_Click(object? sender, EventArgs e) => PromptAndAddEntry("General");
    }

    public class CustomColorTable : ProfessionalColorTable
    {
        public Color ThemeBackgroundColor { get; set; } = SystemColors.Control;

        public override Color ToolStripBorder => Color.Transparent;
        public override Color ToolStripGradientBegin => ThemeBackgroundColor;
        public override Color ToolStripGradientMiddle => ThemeBackgroundColor;
        public override Color ToolStripGradientEnd => ThemeBackgroundColor;
    }
}