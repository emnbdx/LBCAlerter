using System;
using System.Windows.Forms;
using EMToolBox.Job;
using LBCAlerter.Properties;
using LBCMapping;
using LBCMapping.Alerter;
using LBCMapping.Saver;
using LBCAlerter.Identifier;
using JR.Utils.GUI.Forms;

namespace LBCAlerter
{
    public partial class SearchControl : UserControl
    {
        private static System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchControl));

        private SearchJob m_search;
        private RandomJobLauncher m_job;
        private LicenseInfo m_license;

        public SearchJob Search
        {
            get { return m_search; }
            set { m_search = value; }
        }

        private void PrepareForWork()
        {
            //set alerter
            if (Settings.Default.AlertMode == "mail")
                m_search.AddAlerter(new MailAlerter(Settings.Default.MailTo,
                                        String.Format(resources.GetString("MailSubject"), m_search.Keyword)));
            else if (Settings.Default.AlertMode == "rss")
                m_search.AddAlerter(new RSSAlerter());
            else
                m_search.AddAlerter(new LogAlerter());

            //set save mode
            if (Settings.Default.SaveMode == "settings")
                m_search.SetSaveMode(new SettingsSaver(m_search.Keyword.Replace(' ', '+')));
            else
                m_search.SetSaveMode(new FileSaver(m_search.Keyword.Replace(' ', '+')));
        }

        public SearchControl(LicenseInfo license, SearchJob search)
        {
            InitializeComponent();

            m_license = license;
            m_search = search;

            lbKeyWord.Text = m_search.Keyword;
            if (m_license.Status == LicenseStatus.Trial)
                nudRefreshTime.Value = 60;
            else
                nudRefreshTime.Value = 5;
        }

        private void nudRefreshTime_ValueChanged(object sender, EventArgs e)
        {
            if (m_license.Status == LicenseStatus.Trial && nudRefreshTime.Value < 60 )
            {
                FlexibleMessageBox.Show(resources.GetString("TrialTimeLimit").Replace(@"\line", Environment.NewLine));
                nudRefreshTime.Value = 60;
            }

            if (m_job != null)
                m_job.UpdateIntervalTime((int)nudRefreshTime.Value);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (m_job == null)
            {
                PrepareForWork();
                m_job = new RandomJobLauncher(m_search, (int)nudRefreshTime.Value);
            }

            if (!m_job.Started)
            {
                m_job.Start();
                btnStartStop.Text = resources.GetString("Stop");
            }
            else
            {
                m_job.Stop();
                btnStartStop.Text = resources.GetString("Start");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (m_job != null && m_job.Started)
                m_job.Stop();

            SearchDeleted.Invoke(m_search, null);
        }

        public event EventHandler SearchDeleted;
    }
}