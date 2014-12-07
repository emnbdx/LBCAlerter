// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAlerter.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Display alert in log file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCService.Alerter
{
    using LBCMapping;
    using log4net;

    using Newtonsoft.Json.Linq;

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
        public void Alert(JObject ad)
        {
            Log.Info("Nouvelle annonce [" + ad["Title"] + "]");
        }
    }
}