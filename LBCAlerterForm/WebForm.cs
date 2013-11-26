using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LBCMapping.Criteria;
using System.IO;
using LBCAlerter.Properties;
using System.Net;

namespace LBCAlerter
{
    public partial class WebForm : Form
    {
        private string m_regionPath;
        bool redirected = false;

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
        
        public WebForm()
        {
            InitializeComponent();

            //Set size for first page
            ResizeForm(800, 550);
        }

        private void WebForm_Load(object sender, EventArgs e)
        {
            String homePageHtml = CriteriaGetter.GetHomePage();
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
                        String criteriaPageHtml = CriteriaGetter.GetCriteriaPage(wb.Url.AbsolutePath);
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
                        if (MessageBox.Show(Resources.SearchWarning, Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            m_searchUrl = wb.Url.PathAndQuery;
                        else
                        {
                            String criteriaPageHtml = CriteriaGetter.GetCriteriaPage(wb.Url.PathAndQuery);
                            this.webBrowser1.Document.Write(criteriaPageHtml);
                        }
                    }
                    else
                    {
                        m_searchUrl = wb.Url.PathAndQuery;
                    }
                }
            }

            if (m_searchUrl != null)
                this.Close();
        }
    }
}
