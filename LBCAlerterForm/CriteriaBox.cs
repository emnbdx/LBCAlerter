using System;
using System.Drawing;
using System.Windows.Forms;
using LBCMapping;

namespace LBCAlerter
{
    public partial class CriteriaBox : Form
    {
        private static System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CriteriaBox));

        private string m_regionPath;
        private bool redirected = false;

        private string m_searchUrl;

        public string SearchUrl
        {
            get { return m_searchUrl; }
            set { m_searchUrl = value; }
        }

        private void ResizeForm(int width, int height)
        {
            Size = MinimumSize = MaximumSize = new Size(width, height);
        }

        public CriteriaBox()
        {
            InitializeComponent();

            //Set size for first page
            ResizeForm(800, 550);
        }

        private void WebForm_Load(object sender, EventArgs e)
        {
            String homePageHtml = HtmlParser.GetHomePage();
            this.webBrowser1.Document.Write(homePageHtml);
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (sender is WebBrowser)
            {
                WebBrowser wb = sender as WebBrowser;
                if (!redirected && wb.Url != null)
                {
                    if (String.IsNullOrEmpty(m_regionPath))
                        m_regionPath = wb.Url.AbsolutePath;

                    if (!String.IsNullOrEmpty(m_regionPath))
                    {
                        String criteriaPageHtml = HtmlParser.GetCriteriaPage(wb.Url.AbsolutePath);
                        this.webBrowser1.Document.Write(criteriaPageHtml);

                        //Resize to display criteria
                        ResizeForm(1010, 375);

                        redirected = true;
                    }
                }
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (sender is WebBrowser)
            {
                WebBrowser wb = sender as WebBrowser;
                if (wb.Url != null && wb.Url.ToString().Contains("?f=a&th=1"))
                {
                    //User click on search if no keyword alert
                    if (!wb.Url.ToString().Contains("&q="))
                    {
                        if (MessageBox.Show(resources.GetString("SearchWarning"), resources.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            m_searchUrl = Uri.UnescapeDataString(wb.Url.PathAndQuery);
                        else
                        {
                            String criteriaPageHtml = HtmlParser.GetCriteriaPage(wb.Url.PathAndQuery);
                            this.webBrowser1.Document.Write(criteriaPageHtml);
                        }
                    }
                    else
                    {
                        m_searchUrl = Uri.UnescapeDataString(wb.Url.PathAndQuery);
                    }
                }
            }

            if (m_searchUrl != null)
                this.Close();
        }
    }
}