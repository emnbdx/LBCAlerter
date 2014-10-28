namespace LBCService.Alerter
{
    using LBCMapping;
    using log4net;

    /// <summary>
    /// Display alert in log file
    /// </summary>
    public class LogAlerter : IAlerter
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogAlerter));

        /// <summary>
        /// The alert.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        public void Alert(Ad ad)
        {
            Log.Info("Nouvelle annonce [" + ad.Title + "]");
        }
    }
}