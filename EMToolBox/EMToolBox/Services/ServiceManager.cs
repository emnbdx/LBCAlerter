using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Configuration.Install;
using EMToolBox.Services.Installer;

namespace EMToolBox.Services
{
    /// <summary>
    /// Embarque les fonctions de gestion d'un service (installation, désinstallation, configuration...) dans
    /// un exécutable.
    /// </summary>
    /// <example>
    /// static class Program
    /// {
    ///     /// <summary>
    ///     /// Point d'entrée principal de l'application.
    ///     /// </summary>
    ///     static void Main()
    ///     {
    ///         ServiceManager(typeof(MyService)).Run();
    ///     }
    /// }
    /// </example>
    public class ServiceManager
    {
        private static ILog log = LogManager.GetLogger(typeof(ServiceManager));

        #region Private

        /// <summary>
        /// Démarre le service depuis le gestionnaire de services.
        /// </summary>
        /// <param name="serviceType"></param>
        private static void RunService(Type serviceType)
        {
            try
            {
                TraceServiceStart(serviceType, "Service");
                try
                {
                    DebuggableService service = new DebuggableService(serviceType);
                    service.ServiceName = serviceType.ToString();
                    try
                    {
                        ServiceBase.Run(new ServiceBase[] { service });
                    }
                    catch (Exception e)
                    {
                        log.Error("# Arrêt du service suite à une erreur non gérée.", e);
                        service.Crash(e);
                    }
                }
                catch (Exception e)
                {
                    log.Error("# Arrêt du service: Impossible d'initialiser l'instance.", e); 
                }

                TraceServiceStop(serviceType);
            }
            catch (Exception e)
            {
                string message =
                    "#------------------------------------------" + Environment.NewLine +
                    "# Une erreur non gérée a interrompu le démarrage du service." + Environment.NewLine +
                    "#   Service : " + serviceType.ToString() + " [" + Application.ExecutablePath + "]" + Environment.NewLine +
                    e.ToString() + Environment.NewLine +
                    "#------------------------------------------";
                EventLog eventLog = new EventLog();
                eventLog.Source = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
                eventLog.Log = "Application";
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Démarre le service depuis la console (en mode debug)
        /// </summary>
        /// <param name="serviceType"></param>
        private static void RunDebug(Type serviceType)
        {
            try
            {
                TraceServiceStart(serviceType, "Debug");
                try
                {
                    DebuggableService service = new DebuggableService(serviceType);
                    service.ServiceName = serviceType.ToString();
                    try
                    {
                        service.StartDebug();

                        Console.TreatControlCAsInput = true;
                        while (true)
                        {
                            if (Console.KeyAvailable)
                            {
                                ConsoleKeyInfo cki = Console.ReadKey(true);
                                if ((cki.Key == ConsoleKey.C) && ((cki.Modifiers & ConsoleModifiers.Control) != 0))
                                {
                                    log.Info("Control-C intercepté...");
                                    service.StopDebug();
                                }
                            }
                            else
                            {
                                if (service.WaitForEnding(TimeSpan.FromMilliseconds(200))) break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("# Arrêt du service suite à une erreur non gérée.", e);
                        service.Crash(e);
                    }
                }
                catch (Exception e)
                {
                    log.Error("# Arrêt du service: Impossible d'initialiser l'instance.", e);
                }

                TraceServiceStop(serviceType);
            }
            catch (Exception e)
            {
                string message =
                    "#------------------------------------------" + Environment.NewLine +
                    "# Une erreur non gérée a interrompu le démarrage du service." + Environment.NewLine +
                    "#   Service : " + serviceType.ToString() + " [" + Application.ExecutablePath + "]" + Environment.NewLine +
                    e.ToString() + Environment.NewLine +
                    "#------------------------------------------";
            }
        }

        private static void RunConfigurationUI(Type serviceType)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                Application.Run(new ServiceManagerDialog(serviceType));
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        /// <summary>
        /// Affiche l'aide en ligne de commande
        /// </summary>
        private static string GetCommandLineHelp()
        {
            string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            string message =
                Environment.NewLine +
                "  {0} " + Environment.NewLine +
                Environment.NewLine +
                "Usage:" + Environment.NewLine +
                Environment.NewLine +
                "  {0}.exe [options]" + Environment.NewLine +
                Environment.NewLine +
                "    Options:" + Environment.NewLine +
                Environment.NewLine +
                "      -help (-h, -?)" + Environment.NewLine +
                "           Affiche cet écran d'aide" + Environment.NewLine +
                Environment.NewLine +
                "      -debug (-d)" + Environment.NewLine +
                "           Lance le débogage dans la console" + Environment.NewLine +
                Environment.NewLine +
                "      -install (-i)" + Environment.NewLine +
                "           Installation dans le gestionnaire de services." + Environment.NewLine +
                Environment.NewLine +
                "      -label (-l) <libellé_du_service>" + Environment.NewLine +
                "           Spécifie le libellé du service dans le gestionnaire de services." + Environment.NewLine +
                Environment.NewLine +
                "      -name (-n) <nom_du_compte_utilisateur>" + Environment.NewLine +
                "           Spécifie le nom du compte utilisateur à utiliser pour le démarrage" + Environment.NewLine +
                "           du service. Le mot de passe d'ouverture de session est spécifié à" + Environment.NewLine +
                "           l'aide de l'option '-password'." + Environment.NewLine +
                "           Les comptes 'LocalSystem', 'LocalService' et 'NetworkService'" + Environment.NewLine +
                "           peuvent également être spécifiés ici. Dans ce cas le mot de passe ne" + Environment.NewLine +
                "           doit pas être renseigné." + Environment.NewLine +
                "           La valeur par défaut est 'LocalSystem'." + Environment.NewLine +
                "           A utiliser uniquement avec l'option '-install'" + Environment.NewLine +
                Environment.NewLine +
                "      -password (-p) <mot_de_passe>" + Environment.NewLine +
                "           Spécifie le mot de passe d'ouverture de session." + Environment.NewLine +
                "           A utiliser uniquement avec l'option '-install' et si un compte" + Environment.NewLine +
                "           utilisateur a été spécifié avec l'option '-name'." + Environment.NewLine +
                Environment.NewLine +
                "      -uninstall (-u)" + Environment.NewLine +
                "           Désinstallation du gestionnaire de service." + Environment.NewLine +
                Environment.NewLine +
                "      -InstallLog <nom_du_fichier_log>" + Environment.NewLine +
                "           Spécifie le nom du fichier dans lequel seront enregistrées des logs" + Environment.NewLine +
                "           d'installation et de désinstallation." + Environment.NewLine +
                "           Le nom pas défaut est:" + Environment.NewLine +
                "                   {0}.install.log" + Environment.NewLine +
                Environment.NewLine +
                "    Remarque: Si aucune option n'est spécifiée sur la ligne de commande," + Environment.NewLine +
                "       l'interface utilisateur de configuration et de débogage s'affiche." + Environment.NewLine +
                Environment.NewLine +
                "Exemples:" + Environment.NewLine +
                Environment.NewLine +
                "  C:\\> {0}.exe -debug" + Environment.NewLine +
                Environment.NewLine +
                "  C:\\> {0}.exe -i -n domaine/utilisateur -p monmotdepasse" + Environment.NewLine;
            return string.Format(message, appName);
        }

        /// <summary>
        /// Affiche le texte d'aide sur l'invite de commande
        /// </summary>
        private static void RunCommandLineHelp()
        {
            Console.Write(GetCommandLineHelp());
        }

        /// <summary>
        /// Installe le service en ligne de commande
        /// </summary>
        private static void RunCommandLineInstall()
        {
            string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            string displayName = CommandLine.GetValue(new string[] { "label", "l" });
            string userName = CommandLine.GetValue(new string[] { "name", "n" });
            string password = CommandLine.GetValue(new string[] { "password", "p" });

            if (string.IsNullOrEmpty(displayName)) displayName = appName;
            if (string.IsNullOrEmpty(userName)) userName = "LocalSystem";

            ServiceManager.Install(displayName, userName, password);
        }

        /// <summary>
        /// Désinstallation du service en ligne de commande
        /// </summary>
        private static void RunCommandLineUninstall()
        {
            ServiceManager.Uninstall();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            log.Error(e.Exception);
        }
        
        #endregion
        #region Internal

        /// <summary>
        /// Trace les informations de démarrage du service dans la log (numéro de version...)
        /// </summary>
        internal static void TraceServiceStart(Type serviceType, string mode)
        {
            log.Info(
                "#------------------------------------------------------------------------------" + Environment.NewLine +
                "# Démarrage du service [" + serviceType.ToString() + "]" + Environment.NewLine +
                "#   Système   : " + Environment.OSVersion.ToString() + Environment.NewLine +
                "#   Executable: " + Application.ExecutablePath + Environment.NewLine +
                "#   Version   : v" + Application.ProductVersion + Environment.NewLine +
                "#   Mode      : [" + mode + "]");
        }

        /// <summary>
        /// Trace les informations d'arrêt du service dans la log
        /// </summary>
        internal static void TraceServiceStop(Type serviceType)
        {
            log.Info(
                "# Service arrêté [" + serviceType.ToString() + "]" + Environment.NewLine +
                "#------------------------------------------------------------------------------");
        }

        #endregion
        #region Public

        /// <summary>
        /// Démarre le service ou affiche la fenêtre de configuration en fonction
        /// des options de la ligne de commande
        /// </summary>
        /// <param name="serviceType">Type du service à gérer. Ce type doit être un dérivé de 'SimpleService'</param>
        public static void Run(Type serviceType)
        {
            try
            {
                bool isRightServiceType =
                    (serviceType == typeof(SimpleService))
                    || (serviceType.IsSubclassOf(typeof(SimpleService)));
                if (!isRightServiceType)
                    throw new ArgumentException("Le type doit être un dérivé de la classe 'EMToolBox.Services.SimpleService'", "serviceType");

                if (CommandLine.FirstSwitchIs(new string[] { "help", "h", "?" }))
                    RunCommandLineHelp();
                else if (CommandLine.FirstSwitchIs(new string[] { "install", "i", "installation" }))
                    RunCommandLineInstall();
                else if (CommandLine.FirstSwitchIs(new string[] { "uninstall", "u", "desinstallation" }))
                    RunCommandLineUninstall();
                else if (CommandLine.FirstSwitchIs(new string[] { "service", "s" }))
                    RunService(serviceType);
                else if (CommandLine.FirstSwitchIs(new string[] { "debug", "d" }))
                    RunDebug(serviceType);
                else
                    RunConfigurationUI(serviceType);

                Environment.ExitCode = 0;
            }
            catch (Exception e)
            {
                log.Error(e);
                Environment.ExitCode = -1;
            }
        }

        /// <summary>
        /// Installe ou désinstalle le service du gestionnaire de services
        /// </summary>
        /// <param name="displayName">Libellé dans le gestionnaire de services</param>
        /// <param name="accountUserName">Nom du compte utilisateur pour ouverture de session du service</param>
        /// <param name="accountPassword">Mot de passe du compte utilisateur pour ouverture de session du service</param>
        public static void Install(string displayName, string accountUserName, string accountPassword)
        {
            Install(displayName, Path.GetFileNameWithoutExtension(Application.ExecutablePath), accountUserName, accountPassword);
        }

        /// <summary>
        /// Installe ou désinstalle le service du gestionnaire de services
        /// </summary>
        /// <param name="displayName">Libellé dans le gestionnaire de services</param>
        /// <param name="accountUserName">Nom du compte utilisateur pour ouverture de session du service</param>
        /// <param name="accountPassword">Mot de passe du compte utilisateur pour ouverture de session du service</param>
        public static void Install(string displayName, string internalName, string accountUserName, string accountPassword)
        {
            /*
            TransactedInstaller installer = new TransactedInstaller();
            installer.Context = new InstallContext(logFilePath, new string[0]);
            // La classe AssemblyInstaller va spécifier au ServiceInstaller l'assembly correspondant
            // au service
            installer.Installers.Add(new AssemblyInstaller(Assembly.GetAssembly(serviceType)));
            // ou
            installer.Context.Parameters["assemblypath"] = Assembly.GetAssembly(serviceType).Location;
            // ou
            installer.Context.Parameters["assemblypath"] = Application.ExecutablePath + " -service";

            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            installer.Installers.Add(spi);

            ServiceInstaller si = new ServiceInstaller();
            installer.Installers.Add(si);
             */

            // Détermination du type de compte d'ouverture de session
            ServiceAccount serviceAccount = ServiceAccount.LocalSystem;
            if (!string.IsNullOrEmpty(accountUserName))
            {
                if (accountUserName.ToUpper() == "LOCALSYSTEM")
                {
                    serviceAccount = ServiceAccount.LocalSystem;
                    accountUserName = null;
                }
                else if (accountUserName.ToUpper() == "LOCALSERVICE")
                {
                    serviceAccount = ServiceAccount.LocalService;
                    accountUserName = null;
                }
                else if (accountUserName.ToUpper() == "NETWORKSERVICE")
                {
                    serviceAccount = ServiceAccount.NetworkService;
                    accountUserName = null;
                }
                else
                    serviceAccount = ServiceAccount.User;
            }


            string installerLogFileName = Application.ExecutablePath + ".install.log";
            if (CommandLine.ContainsSwitch("InstallLog"))
            {
                installerLogFileName = CommandLine.GetValue("InstallLog");
                if (string.IsNullOrEmpty(installerLogFileName)) throw new Exception("Le paramètre 'InstallLog' a été précisé sur la ligne de commande mais aucun nom de fichier n'a été spécifié");
            }

            TransactedInstaller installer = new TransactedInstaller();
            installer.Context = new InstallContext(installerLogFileName, Environment.GetCommandLineArgs());
            installer.Context.Parameters["assemblypath"] = Application.ExecutablePath;

            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            spi.Account = serviceAccount;
            spi.Username = accountUserName;
            spi.Password = accountPassword;
            installer.Installers.Add(spi);

            ServiceInstallerEx si = new ServiceInstallerEx();
            si.ServiceName = internalName;
            si.DisplayName = displayName;
            si.CommandLineOptions = "-service";
            installer.Installers.Add(si);

            installer.Install(new Hashtable());
        }


        /// <summary>
        /// Installe ou désinstalle le service du gestionnaire de services
        /// </summary>
        /// <param name="uninstall"></param>
        public static void Uninstall()
        {
            Uninstall(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
        }

        /// <summary>
        /// Installe ou désinstalle le service du gestionnaire de services
        /// </summary>
        /// <param name="uninstall"></param>
        public static void Uninstall(string internalName)
        {
            string installerLogFileName = Application.ExecutablePath + ".install.log";
            if (CommandLine.ContainsSwitch("InstallLog"))
            {
                installerLogFileName = CommandLine.GetValue("InstallLog");
                if (string.IsNullOrEmpty(installerLogFileName)) throw new Exception("Le paramètre 'InstallLog' a été précisé sur la ligne de commande mais aucun nom de fichier n'a été spécifié");
            }

            TransactedInstaller installer = new TransactedInstaller();
            installer.Context = new InstallContext(installerLogFileName, Environment.GetCommandLineArgs());
            installer.Context.Parameters["assemblypath"] = Application.ExecutablePath;

            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            installer.Installers.Add(spi);

            ServiceInstallerEx si = new ServiceInstallerEx();
            si.ServiceName = internalName;
            installer.Installers.Add(si);

            installer.Uninstall(null);
        }
        
        #endregion
    }
}
