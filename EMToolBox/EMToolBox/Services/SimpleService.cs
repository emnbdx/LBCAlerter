using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EMToolBox.Services
{
    /// <summary>
    /// Classe abstraite utilisé par TimedService, il suffit de dérivé de cette classe pour obtenir un service timé
    /// L'intervalle est paramétrable via l'attribut Interval
    /// </summary>
    public abstract class SimpleService
    {
        private DebuggableService m_service = null;

        private bool m_stopping = false;

        internal void StopProcessing()
        {
            bool firstStopRequest;
            lock (this)
            {
                firstStopRequest = !m_stopping; // Was the service already stopping or is it the first stop request received?
                m_stopping = true;
            }

            // First stop request? This test is to garantee that the 'Finalize()' method won't be called more than once...
            if (firstStopRequest) Terminate();
        }

        internal DebuggableService Service { get { return m_service; } set { m_service = value; } }

        /// <summary>
        /// Constructeur
        /// </summary>
        public SimpleService()
        {
            this.ThreadApartmentState = ApartmentState.MTA;
            this.Interval = 1 * 1000;
        }

        /// <summary>
        /// Le service est en cours d'arrêt
        /// </summary>
        protected bool Stopping { get { lock (this) { return m_stopping; } } }

        /// <summary>
        /// Virtuelle appelée au moment de l'initialisation du service, avant le premier appel
        /// à la méthode 'Process()'
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Virtuelle appelée à chaque expiration de l'intervalle de déclenchement du service
        /// </summary>
        public virtual void Process() { }

        /// <summary>
        /// Virtuelle appelée à chaque expiration de l'intervalle de déclenchement du service.
        /// </summary>
        /// <param name="firstProcess">True s'il s'agit du premier appel de la méthode</param>
        public virtual void Process(bool firstProcess) { }

        /// <summary>
        /// Virtuelle appelée lors de la demande d'arrêt du service
        /// </summary>
        public virtual void Terminate() { }

        /// <summary>
        /// Appelée lors d'un crash du service
        /// </summary>
        /// <param name="e"></param>
        public virtual void Crash(Exception e) { }

        /// <summary>
        /// Demande d'arrêt au gestionnaire de services
        /// </summary>
        public void Stop()
        {
            m_service.Stop();
        }

        /// <summary>
        /// Le service fonctionne actuellement en mode debug
        /// </summary>
        public bool Debugging { get { return m_service.Debugging; } }

        /// <summary>
        /// Intervalle de déclenchement du service
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Define the apartment state of the main thread of the service
        /// </summary>
        public virtual ApartmentState ThreadApartmentState { get; private set; }
    }
}
