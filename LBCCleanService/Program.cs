namespace LBCCleanService
{
    using EMToolBox.Services;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            ServiceManager.Run(typeof(CleanService));
        }
    }
}
