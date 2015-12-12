using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using log4net;

namespace EMToolBox.Services
{
    public partial class ServiceDebugDialog : Form
    {
        private static ILog log = LogManager.GetLogger(typeof(ServiceDebugDialog));
        
        private Type m_serviceType;

        private AutoResetEvent m_stopEvent = new AutoResetEvent(false);

        private Thread m_serviceThread = null;

        public ServiceDebugDialog(Type serviceType)
        {
            InitializeComponent();
            m_serviceType = serviceType;
            /*Log.Listeners.Add(new DelegateLogListener("Console", LogSeverity.All, new LogAvailableEventHandler(ServiceThread_LogAvailable)));
            this.LogAvailable += new LogAvailableEventHandler(Form_LogAvailable);*/

            bool initialized = false;

            if (!log.Logger.Repository.Configured)
            {
                log4net.Config.XmlConfigurator.Configure();
                txtConsole.AppendText("WARNING: log4net not automatically configured. " +
                    "Check AssemblyInfo.cs for - " +
                    "[assembly: log4net.Config.XmlConfigurator(Watch=true)]\r\n");
            }

            foreach (log4net.Appender.IAppender appender in log.Logger.Repository.GetAppenders())
            {
                if (appender.GetType() == typeof(DelegateAppender))
                {
                    DelegateAppender delegateAppender = (DelegateAppender)appender;
                    // .NET 2.0+
                    delegateAppender.LogTextMethod = this.AddStatus;
                    // .NET 1.1
                    //delegateAppender.LogTextMethod = new log4net.Appender.LogTextAppend(this.AddStatus);
                    initialized = true;
                }
            }

            if (!initialized)
            {
                txtConsole.AppendText("ERROR: Unable to add DelegateAppender to logging!\r\n");
            }
        }

        #region Traces

        private List<string> m_consoleLines = new List<string>();

        public void AddStatus(string message)
        {
            string[] newEntries = message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            m_consoleLines.AddRange(newEntries);
            while (m_consoleLines.Count > 500) m_consoleLines.RemoveAt(0);

            txtConsole.SuspendLayout();
            try
            {
                string consoleText = string.Join("\r\n", m_consoleLines.ToArray());
                
                if (this.InvokeRequired)
                {
                    IAsyncResult result = BeginInvoke(new MethodInvoker(delegate()
                    {
                        txtConsole.Text = consoleText;
                        txtConsole.Focus();

                        int selectionStart = txtConsole.Text.Length - 1;
                        if (m_consoleLines.Count > 0) selectionStart -= m_consoleLines[m_consoleLines.Count - 1].Length - 2;
                        txtConsole.SelectionStart = selectionStart;
                        txtConsole.ScrollToCaret();
                    }));

                    // wait until invocation is completed
                    EndInvoke(result);
                }
                else if (this.IsHandleCreated)
                {
                    txtConsole.Text = consoleText;
                    txtConsole.Focus();

                    int selectionStart = txtConsole.Text.Length - 1;
                    if (m_consoleLines.Count > 0) selectionStart -= m_consoleLines[m_consoleLines.Count - 1].Length - 2;
                    txtConsole.SelectionStart = selectionStart;
                    txtConsole.ScrollToCaret();
                }
            }
            finally
            {
                txtConsole.ResumeLayout();
            }
        }

        #endregion Traces

        private void ServiceThread()
        {
            using (DebuggableService service = new DebuggableService(m_serviceType))
            {
                service.DebugStopped += new EventHandler(service_DebugStopped);
                service.StopQuery += new EventHandler(service_StopQuery);
                ServiceManager.TraceServiceStart(m_serviceType, "Debug");
                try
                {
                    service.StartDebug();
                    m_stopEvent.WaitOne();
                    service.StopDebug();
                }
                catch (Exception e)
                {
                    log.Error("# Arrêt du service suite à une erreur non gérée.", e);
                    service.Crash(e);
                }
                ServiceManager.TraceServiceStop(m_serviceType);
            }
        }

        void service_StopQuery(object sender, EventArgs e)
        {
            m_stopEvent.Set();
        }

        private void service_DebugStopped(object sender, EventArgs e)
        {
            this.Invoke(new EventHandler(Invoked_ServiceStopped));
        }

        private void Invoked_ServiceStopped(object sender, EventArgs e)
        {
            btnStopService.Enabled = false;
            btnStartService.Enabled = true;
        }

        private void StartService()
        {
            // Force la création du handle de fenêtre
            IntPtr handle = this.Handle;

            try
            {
                btnStartService.Enabled = false;
                m_serviceThread = new Thread(new ThreadStart(ServiceThread));
                m_serviceThread.Start();
                btnStopService.Enabled = true;
            }
            catch
            {
                btnStartService.Enabled = true;
            }
        }

        private void StopService()
        {
            this.ServiceStopped += new EventHandler(Form_ServiceStopped);
            Thread waitServiceEndingThread = new Thread(new ThreadStart(WaitServiceEndingThread));
            waitServiceEndingThread.Start();
        }

        private void WaitServiceEndingThread()
        {
            m_stopEvent.Set();
            m_serviceThread.Join();
            m_serviceThread = null;
            this.Invoke(this.ServiceStopped);
        }

        private event EventHandler ServiceStopped;

        private void Form_ServiceStopped(object sender, EventArgs ea)
        {
            btnStartService.Enabled = true;
        }

        private bool IsServiceRunning()
        {
            return m_serviceThread != null;
        }

        private bool QueryStopService()
        {
            bool result = true;

            if (IsServiceRunning())
            {
                result = false;

                DialogResult dr = MessageBox.Show(
                    this,
                    "Stop debug service?",
                    this.Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    StopService();
                    result = true;
                }
            }

            return result;
        }

        private void ClearConsole()
        {
            txtConsole.Clear();
        }

        #region Form event handlers

        private void btnStartService_Click(object sender, EventArgs e)
        {
            StartService();
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            QueryStopService();
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            ClearConsole();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ServiceDebugDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            if (QueryStopService())
            {
                DialogResult dr = MessageBox.Show(
                    this,
                    "Close debug console?",
                    this.Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    e.Cancel = false;
                }
            }
        }

        #endregion Form event handlers
    }
}