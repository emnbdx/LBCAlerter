using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LBCAlerter.Properties;
using LBCMapping;
using LBCAlerter.Identifier;
using EMToolBox.License;
using JR.Utils.GUI.Forms;
using System.Threading;

namespace LBCAlerter
{
    public partial class MainForm : Form
    {
        #region private

        private static System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private LicenseInfo m_license;

        private List<SearchJob> GetSearchList()
        {
            List<SearchJob> searchList = new List<SearchJob>();

            Control[] controls = flowLayoutPanel1.Controls.Find("SearchControl", true);
            foreach (Control control in controls)
            {
                SearchControl searchControl = control as SearchControl;
                searchList.Add(searchControl.Search);
            }

            return searchList;
        }

        private void AddSearchToList(SearchJob s)
        {
            SearchControl searchControl = new SearchControl(m_license, s);
            searchControl.SearchDeleted += new EventHandler(searchControl_SearchDeleted);
            flowLayoutPanel1.Controls.Add(searchControl);
        }

        private void SaveSettings()
        {
            Settings.Default.SaveMode = (string)cbSaveMode.SelectedItem;
            Settings.Default.AlertMode = (string)cbAlertMode.SelectedItem;
            Settings.Default.Save();

            SearchJobSerializer.Save(GetSearchList());
        }

        private void DisplayAboutBox()
        {
            string baseMessage = resources.GetString("About").Replace(@"\line", Environment.NewLine);
            string licenseInfo = resources.GetString("LicenseInfo").Replace(@"\line", Environment.NewLine);
            string demo = resources.GetString("Demo");
            string complete = resources.GetString("Complete");

            string about;
            if (m_license.Status == LicenseStatus.Trial || m_license.Status == LicenseStatus.FullLimited)
            {
                licenseInfo = String.Format(licenseInfo, m_license.ExpireDate, m_license.DaysLeft);
                about = String.Format(baseMessage, Constant.MachineCode, m_license.Status == LicenseStatus.Trial ? demo : complete, licenseInfo);
            }
            else
                about = String.Format(baseMessage, Constant.MachineCode, complete, "");

            string releaseNote = resources.GetString("ReleaseNote").Replace(@"\line", Environment.NewLine);
            
            richTextBox1.Clear();
            richTextBox1.AppendText(about + Environment.NewLine + Environment.NewLine);
            richTextBox1.AppendText(releaseNote);
        }

        private void GetLicense()
        {
            m_license = new IdentifySoapClient().Validate("lbcalerter", Constant.MachineCode);  
        }

        #endregion private

        #region public

        public MainForm(LicenseInfo license)
        {
            InitializeComponent();
            m_license = license;

            // Hide console
            ShowWindow(GetConsoleWindow(), SW_HIDE);
            btnShowConsole.Enabled = true;
            btnHideConsole.Enabled = false;

            if (!String.IsNullOrEmpty(Settings.Default.SaveMode))
                cbSaveMode.SelectedIndex = cbSaveMode.Items.IndexOf(Settings.Default.SaveMode);
            else
                cbSaveMode.SelectedIndex = 0;

            if (!String.IsNullOrEmpty(Settings.Default.AlertMode))
                cbAlertMode.SelectedIndex = cbAlertMode.Items.IndexOf(Settings.Default.AlertMode);
            else
                cbAlertMode.SelectedIndex = 0;

            List<SearchJob> searchs = SearchJobSerializer.Load();
            foreach (SearchJob search in searchs)
                AddSearchToList(search);

            DisplayAboutBox();

            if (Settings.Default.AlertMode == "mail" && String.IsNullOrEmpty(Settings.Default.MailTo))
            {
                FlexibleMessageBox.Show(resources.GetString("MailWarning").Replace(@"\line", Environment.NewLine), "");
            }

            licTimer.Interval = (1000 * 60 * 30); //30 minutes
            licTimer.Start();
        }

        #endregion public

        #region events

        private void cbAlertMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbMailTo.Visible = tbMailTo.Visible =
            lbMailRetry.Visible = nudMailRetry.Visible = ((string)cbAlertMode.SelectedItem == "mail");

            lbRssFile.Visible = tbRssFile.Visible = btnRssFile.Visible = ((string)cbAlertMode.SelectedItem == "rss");
        }

        private void btnRssFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbRssFile.Text = openFileDialog1.FileName;
            }
        }

        private void btnAddSearch_Click(object sender, EventArgs e)
        {
            if (m_license.Status == LicenseStatus.Trial)
            {
                Control[] controls = flowLayoutPanel1.Controls.Find("SearchControl", true);
                if (controls.Length == 1)
                {
                    FlexibleMessageBox.Show(resources.GetString("TrialSearchLimit").Replace(@"\line", Environment.NewLine));
                    return;
                }
            }

            CriteriaBox criteriaBox = new CriteriaBox();
            criteriaBox.FormClosed += new FormClosedEventHandler(criteriaBox_FormClosed);

            criteriaBox.Show();
        }

        private void criteriaBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            String tmp = (sender as CriteriaBox).SearchUrl;

            if (!String.IsNullOrEmpty(tmp))
            {
                SearchJob search;
                if(m_license.Status == LicenseStatus.Trial)
                    search = new SearchJob(tmp, "", true);
                else
                    search = new SearchJob(tmp, "", true);

                AddSearchToList(search);
            }
        }

        private void searchControl_SearchDeleted(object sender, EventArgs e)
        {
            Control[] controls = flowLayoutPanel1.Controls.Find("SearchControl", true);
            foreach (Control control in controls)
            {
                SearchControl sc = control as SearchControl;
                if (sc.Search == sender as SearchJob)
                    flowLayoutPanel1.Controls.Remove(control);
            }
        }

        private void btnShowConsole_Click(object sender, EventArgs e)
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);
            btnShowConsole.Enabled = false;
            btnHideConsole.Enabled = true;

            this.Focus();
        }

        private void btnHideConsole_Click(object sender, EventArgs e)
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
            btnShowConsole.Enabled = true;
            btnHideConsole.Enabled = false;

            this.Focus();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void licTimer_Tick(object sender, EventArgs e)
        {
            DisplayAboutBox();

            bool exit = false;
            switch (m_license.Status)
            {
                case LicenseStatus.Expired:
                    FlexibleMessageBox.Show("Votre licence est expirée.\r\nVous pouvez commander une licence sur www.eddymontus.fr/blog.php");
                    exit = true;
                    break;

                case LicenseStatus.Unknown:
                    FlexibleMessageBox.Show("Votre licence n'est pas valide.\r\nVous pouvez commander une licence sur www.eddymontus.fr/blog.php");
                    exit = true;
                    break;
                case LicenseStatus.Trial:
                case LicenseStatus.FullLimited:
                case LicenseStatus.Full:
                    break;
            }

            if (exit)
                this.Close();
            
            Thread t = new Thread(GetLicense);
            t.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        #endregion events
    }
}