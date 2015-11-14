namespace LBCCleanService
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;

    using EMToolBox.Services;
    using log4net;

    /// <summary>
    /// The clean service.
    /// </summary>
    public class CleanService : SimpleService
    {
        /// <summary>
        /// The _last execution file.
        /// </summary>
        private const string LastExecutionFile = "lastrun";

        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(CleanService));

        /// <summary>
        /// The ads conservation days.
        /// </summary>
        private int adsConservationDays;

        /// <summary>
        /// The attempts conservation days.
        /// </summary>
        private int attemptsConservationDays;

        /// <summary>
        /// The execution hour.
        /// </summary>
        private int executionHour;

        /// <summary>
        /// The initialize.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            this.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["ServiceInterval"]) * 60 * 1000;

            this.adsConservationDays = Convert.ToInt32(ConfigurationManager.AppSettings["AdsConservationDays"]);
            this.attemptsConservationDays = Convert.ToInt32(ConfigurationManager.AppSettings["AttemptsConservationDays"]);
            this.executionHour = Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionHour"]);

            if (!File.Exists(LastExecutionFile))
            {
                File.WriteAllText(LastExecutionFile, DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// The process.
        /// </summary>
        public override void Process()
        {
            base.Process();

            // Get last date
            var lastExecution = DateTime.MinValue;
            var lastExecutionString = File.ReadAllText(LastExecutionFile);
            try
            {
                lastExecution = DateTime.Parse(lastExecutionString);
            }
            catch (Exception e)
            {
                Log.Error("Erreur lors de la converstion de [" + lastExecutionString + "]", e);
            }

            if (lastExecution == DateTime.MinValue || (DateTime.Now - lastExecution).Days < 1
                || DateTime.Now.Hour != this.executionHour)
            {
                return;
            }

            Log.Info("Lancement du nettoyage");
                
            using (var cnct = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    @"DELETE FROM Ad
                                                    WHERE getdate() - date > " + this.adsConservationDays + @"
                                                    DELETE FROM Attempts
                                                    WHERE getdate() - ProcessDate > " + this.attemptsConservationDays
                                         + @"
                                                    ",
                    cnct);

                try
                {
                    cnct.Open();
                    cmd.ExecuteNonQuery();
                    Log.Info("Nettoyage terminé");
                }
                catch (Exception e)
                {
                    Log.Error("Erreur lors du nettoyage", e);
                }
            }

            File.WriteAllText(LastExecutionFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }
    }
}
