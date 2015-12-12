using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceProcess;
using log4net;

namespace EMToolBox.Services
{
    /// <summary>
    /// Classe dérivé de ServiceBase permettant d'executer un SimpleService à intervalle régulier
    /// Pour l'utiliser il suffit de créer une classe dérivant de SimpleService
    /// 
    /// public class NewService : SimpleService
    /// { 
    ///     public override void Initialize()
    ///     {
    ///         //To set Process() interval
    ///         base.Interval = 1
    ///     }
    ///
    ///     public override void Process()
    ///     {
    ///         ...
    ///     }   
    /// }     
    /// 
    /// et de modifier le Program.cs 
    /// 
    /// static void Main()
    /// {
    ///     ServiceBase.Run(new TimedService(typeof(NewService)));
    /// }
    /// 
    /// </summary>
    partial class DebuggableService : ServiceBase
    {
        private static ILog log = LogManager.GetLogger(typeof(DebuggableService));

        private SimpleService m_simpleService;
        private EventWaitHandle m_stopEvent = null;
        private EventWaitHandle m_threadStoppedEvent = null;
        private Thread m_serviceThread = null;
        private bool m_debugging = false;

        private void OnStopQuery()
        {
            if (StopQuery != null) StopQuery(this, EventArgs.Empty);
        }

        private void OnDebugStopped()
        {
            if (DebugStopped != null) DebugStopped(this, EventArgs.Empty);
        }

        public DebuggableService(Type simpleServiceType)
        {
            InitializeComponent();
            m_simpleService = (SimpleService)Activator.CreateInstance(simpleServiceType);
            m_simpleService.Service = this;
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Démarrage du sevice...");
            m_stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_threadStoppedEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_serviceThread = new Thread(new ThreadStart(ServiceThread));
            m_serviceThread.SetApartmentState(m_simpleService.ThreadApartmentState);
            m_serviceThread.Start();
        }

        protected override void OnStop()
        {
            log.Info("Arret du sevice...");
            m_stopEvent.Set();
            m_simpleService.StopProcessing();
            // Attend la fin de la boucle en cours
            m_threadStoppedEvent.WaitOne();
            m_serviceThread = null;
        }

        private void ServiceThread()
        {
            try
            {
                m_simpleService.Initialize();

                bool firstProcess = true;
                bool finished = false;
                while (!finished)
                {
                    m_simpleService.Process();
                    m_simpleService.Process(firstProcess);
                    firstProcess = false;
                    finished = m_stopEvent.WaitOne(m_simpleService.Interval, true);
                }
                this.ExitCode = 0;
                m_threadStoppedEvent.Set();

                OnDebugStopped(); // Notification requise pour le débogage
            }
            catch (Exception e)
            {
                log.Error("Arrêt du service suite à une erreur non gérée:\r\n", e);
                m_simpleService.Crash(e);
                if (!m_debugging)
                {
                    m_threadStoppedEvent.Set();
                    base.Stop();
                    // Indique au gestionnaire de service Windows que le service ne s'est pas terminé normalement
                    this.ExitCode = 1;
                }
                OnDebugStopped(); // Notification requise pour le débogage
            }

            // A tout hasard, pour être sûr de ne pas bloquer le thread principal...
            m_threadStoppedEvent.Set();
        }

        public void StartDebug()
        {
            m_debugging = true;
            this.OnStart(Environment.GetCommandLineArgs());
        }

        public bool WaitForEnding(TimeSpan timeout)
        {
            return m_threadStoppedEvent.WaitOne(timeout);
        }

        public void StopDebug()
        {
            this.OnStop();
        }

        public new void Stop()
        {
            OnStopQuery();
            if (!m_debugging) base.Stop();
        }

        public void Crash(Exception e)
        {
            m_simpleService.Crash(e);
        }

        public bool Debugging { get { return m_debugging; } }

        public event EventHandler StopQuery;
        public event EventHandler DebugStopped;
    }
}
