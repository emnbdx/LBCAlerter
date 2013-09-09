using System;
namespace LBCAlerter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnAddSearch = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tbMailTo = new System.Windows.Forms.TextBox();
            this.lbMailTo = new System.Windows.Forms.Label();
            this.tbRssFile = new System.Windows.Forms.TextBox();
            this.lbRssFile = new System.Windows.Forms.Label();
            this.btnRssFile = new System.Windows.Forms.Button();
            this.cbAlertMode = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cbSaveMode = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.nudMailRetry = new System.Windows.Forms.NumericUpDown();
            this.lbMailRetry = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnHideConsole = new System.Windows.Forms.Button();
            this.btnShowConsole = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.licTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMailRetry)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnAddSearch);
            this.tabPage2.Controls.Add(this.flowLayoutPanel1);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnAddSearch
            // 
            resources.ApplyResources(this.btnAddSearch, "btnAddSearch");
            this.btnAddSearch.Name = "btnAddSearch";
            this.btnAddSearch.UseVisualStyleBackColor = true;
            this.btnAddSearch.Click += new System.EventHandler(this.btnAddSearch_Click);
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tbMailTo);
            this.tabPage1.Controls.Add(this.lbMailTo);
            this.tabPage1.Controls.Add(this.tbRssFile);
            this.tabPage1.Controls.Add(this.lbRssFile);
            this.tabPage1.Controls.Add(this.btnRssFile);
            this.tabPage1.Controls.Add(this.cbAlertMode);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.cbSaveMode);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.nudMailRetry);
            this.tabPage1.Controls.Add(this.lbMailRetry);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tbMailTo
            // 
            this.tbMailTo.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::LBCAlerter.Properties.Settings.Default, "MailTo", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.tbMailTo, "tbMailTo");
            this.tbMailTo.Name = "tbMailTo";
            this.tbMailTo.Text = global::LBCAlerter.Properties.Settings.Default.MailTo;
            // 
            // lbMailTo
            // 
            resources.ApplyResources(this.lbMailTo, "lbMailTo");
            this.lbMailTo.Name = "lbMailTo";
            // 
            // tbRssFile
            // 
            resources.ApplyResources(this.tbRssFile, "tbRssFile");
            this.tbRssFile.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::LBCAlerter.Properties.Settings.Default, "RssFile", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbRssFile.Name = "tbRssFile";
            this.tbRssFile.Text = global::LBCAlerter.Properties.Settings.Default.RssFile;
            // 
            // lbRssFile
            // 
            resources.ApplyResources(this.lbRssFile, "lbRssFile");
            this.lbRssFile.Name = "lbRssFile";
            // 
            // btnRssFile
            // 
            resources.ApplyResources(this.btnRssFile, "btnRssFile");
            this.btnRssFile.Name = "btnRssFile";
            this.btnRssFile.UseVisualStyleBackColor = true;
            this.btnRssFile.Click += new System.EventHandler(this.btnRssFile_Click);
            // 
            // cbAlertMode
            // 
            resources.ApplyResources(this.cbAlertMode, "cbAlertMode");
            this.cbAlertMode.FormattingEnabled = true;
            this.cbAlertMode.Items.AddRange(new object[] {
            resources.GetString("cbAlertMode.Items"),
            resources.GetString("cbAlertMode.Items1")});
            this.cbAlertMode.Name = "cbAlertMode";
            this.cbAlertMode.SelectedIndexChanged += new System.EventHandler(this.cbAlertMode_SelectedIndexChanged);
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // cbSaveMode
            // 
            resources.ApplyResources(this.cbSaveMode, "cbSaveMode");
            this.cbSaveMode.FormattingEnabled = true;
            this.cbSaveMode.Items.AddRange(new object[] {
            resources.GetString("cbSaveMode.Items"),
            resources.GetString("cbSaveMode.Items1")});
            this.cbSaveMode.Name = "cbSaveMode";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // nudMailRetry
            // 
            resources.ApplyResources(this.nudMailRetry, "nudMailRetry");
            this.nudMailRetry.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::LBCAlerter.Properties.Settings.Default, "MailRetry", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.nudMailRetry.Name = "nudMailRetry";
            this.nudMailRetry.Value = global::LBCAlerter.Properties.Settings.Default.MailRetry;
            // 
            // lbMailRetry
            // 
            resources.ApplyResources(this.lbMailRetry, "lbMailRetry");
            this.lbMailRetry.Name = "lbMailRetry";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnHideConsole);
            this.tabPage3.Controls.Add(this.btnShowConsole);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnHideConsole
            // 
            resources.ApplyResources(this.btnHideConsole, "btnHideConsole");
            this.btnHideConsole.Name = "btnHideConsole";
            this.btnHideConsole.UseVisualStyleBackColor = true;
            this.btnHideConsole.Click += new System.EventHandler(this.btnHideConsole_Click);
            // 
            // btnShowConsole
            // 
            resources.ApplyResources(this.btnShowConsole, "btnShowConsole");
            this.btnShowConsole.Name = "btnShowConsole";
            this.btnShowConsole.UseVisualStyleBackColor = true;
            this.btnShowConsole.Click += new System.EventHandler(this.btnShowConsole_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.richTextBox1);
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // licTimer
            // 
            this.licTimer.Tick += new System.EventHandler(this.licTimer_Tick);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMailRetry)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Button btnHideConsole;
        private System.Windows.Forms.Button btnShowConsole;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnAddSearch;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox tbMailTo;
        private System.Windows.Forms.Label lbMailTo;
        private System.Windows.Forms.TextBox tbRssFile;
        private System.Windows.Forms.Label lbRssFile;
        private System.Windows.Forms.Button btnRssFile;
        private System.Windows.Forms.ComboBox cbAlertMode;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cbSaveMode;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudMailRetry;
        private System.Windows.Forms.Label lbMailRetry;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer licTimer;
    }
}