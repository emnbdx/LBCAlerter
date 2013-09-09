using System;
using System.Windows.Forms;
using log4net;
using LBCAlerter.Identifier;
using EMToolBox.License;
using JR.Utils.GUI.Forms;
using System.Reflection;

namespace LBCAlerter
{
    public class Program
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Entry point
        /// </summary>
        [STAThread]
        private static void Main()
        {            
            log4net.Config.XmlConfigurator.Configure();

            log.Info("-------------------------------------------");
            log.Info("Démarrage de LBCAlerter");
            log.Info("-------------------------------------------");

            log.Info("Vérification de la licence...");
            LicenseInfo license = new IdentifySoapClient().Validate("lbcalerter", Constant.MachineCode);

            bool exit = false;
            switch (license.Status)
            {
                case LicenseStatus.Expired:
                    log.Info("Expirée");
                    FlexibleMessageBox.Show("Votre licence est expirée.\r\nVous pouvez commander une licence sur www.eddymontus.fr/blog.php");
                    exit = true;
                    break;

                case LicenseStatus.Unknown:
                    log.Info("Inconnu");
                    FlexibleMessageBox.Show("Votre licence n'est pas valide.\r\nVous pouvez commander une licence sur www.eddymontus.fr/blog.php");
                    exit = true;
                    break;
                case LicenseStatus.Trial:
                case LicenseStatus.FullLimited:
                    log.Info("Ok, limité");
                    break;
                case LicenseStatus.Full:
                    log.Info("OK, complète");
                    break;
            }

            if (exit)
                return;

            log.Info("---------------------------------------------");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(license));
        }
    }
}