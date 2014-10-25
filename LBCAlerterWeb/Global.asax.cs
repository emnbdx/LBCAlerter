
namespace LBCAlerterWeb
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using LBCAlerterWeb.Models;

    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Init log
            log4net.Config.XmlConfigurator.Configure();

            if (ConfigurationManager.AppSettings["dbMode"] == "debug")
            {
                Database.SetInitializer<ApplicationDbContext>(new DebugInitializer());
            }
            else
            {
                Database.SetInitializer<ApplicationDbContext>(new ProductionInitializer());
            }
        }
    }
}
