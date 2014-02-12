using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LBCAlerterWeb.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Helpers;
using log4net;

namespace LBCAlerterWeb.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Search/
        public ActionResult Index()
        {
            return View();
        }
    }
}
