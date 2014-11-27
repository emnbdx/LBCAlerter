// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCService
{
    using EMToolBox.Services;

    /// <summary>
    /// The program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        public static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            ServiceManager.Run(typeof(NotificationService));
        }
    }
}
