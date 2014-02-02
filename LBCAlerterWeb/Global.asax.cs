using LBCAlerterWeb.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LBCAlerterWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Init log
            log4net.Config.XmlConfigurator.Configure();

            //Debug mode
            Database.SetInitializer<ApplicationDbContext>(new DebugInitializer());
            //Production mode
            //Database.SetInitializer<ApplicationDbContext>(new ProductionInitializer());

            AspNetTimer.Start();
        }
    }
}
