using LBCMapping;
using log4net;

namespace LBCAlerterForm.Alerter
{
    /// <summary>
    /// Display alert in log file
    /// </summary>
    public class LogAlerter : IAlerter
    {
        private static ILog log = LogManager.GetLogger(typeof(LogAlerter));

        public void Alert(Ad ad)
        {
            log.Info("Nouvelle annonce [" + ad.Title + "]");
        }
    }
}