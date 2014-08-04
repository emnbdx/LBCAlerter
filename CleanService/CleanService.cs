using EMToolBox.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCCleanService
{
    public class CleanService : SimpleService
    {
        ILog log = LogManager.GetLogger(typeof(CleanService));

        private string _lastExecutionFile = "lastrun";

        private int _adsConservationDays;
        private int _attemptsConservationDays;
        private int _executionHour;

        public override void Initialize()
        {
            base.Initialize();

            base.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["ServiceInterval"]) * 60 * 1000;

            _adsConservationDays = Convert.ToInt32(ConfigurationManager.AppSettings["AdsConservationDays"]);
            _attemptsConservationDays = Convert.ToInt32(ConfigurationManager.AppSettings["AttemptsConservationDays"]);
            _executionHour = Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionHour"]);

            if(!File.Exists(_lastExecutionFile))
                File.WriteAllText(_lastExecutionFile, DateTime.Now.AddDays(-1).ToString());
        }

        public override void Process()
        {
            base.Process();

            //Get last date
            DateTime lastExecution = DateTime.MinValue;
            String lastExecutionString = File.ReadAllText(_lastExecutionFile);
            try
            {
                lastExecution = DateTime.Parse(lastExecutionString);
            }
            catch(Exception e)
            {
                log.Error("Erreur lors de la converstion de [" + lastExecutionString + "]", e);
            }

            if (lastExecution != DateTime.MinValue &&
                (DateTime.Now - lastExecution).Days >= 1 &&
                DateTime.Now.Hour == _executionHour)
            {
                log.Info("Lancement du nettoyage");
                
                using (SqlConnection cnct = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(@"DELETE FROM Ad
                                                    WHERE getdate() - date > " + _adsConservationDays + @"
                                                    DELETE FROM Attempts
                                                    WHERE getdate() - ProcessDate > " + _attemptsConservationDays + @"
                                                    ", cnct);

                    try
                    {
                        cnct.Open();
                        cmd.ExecuteNonQuery();
                        log.Info("Nettoyage terminé");
                    }
                    catch (Exception e)
                    {
                        log.Error("Erreur lors du nettoyage", e);
                    }
                }

                File.WriteAllText(_lastExecutionFile, DateTime.Now.ToString());
            }
        }
    }
}
