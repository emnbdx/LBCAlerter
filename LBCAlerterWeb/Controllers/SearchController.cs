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
using System.Web.Security;

namespace LBCAlerterWeb.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private static ILog log = LogManager.GetLogger(typeof(SearchController));

        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager { get; set; }

        public SearchController()
        {
            db = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }       

        // GET: /Search/
        public async Task<ActionResult> Index()
        {
            var currentUser = await userManager.FindByIdAsync(User.Identity.GetUserId());
            return View(db.Searches.ToList().Where(search => search.User.Id == currentUser.Id));
        }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> All()
        {
            return View(await db.Searches.ToListAsync());
        }

        // GET: /Search/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DisplaySecondPage(string url)
        {
            String htmlCode = LBCMapping.HtmlParser.GetCriteriaPage(url);
            return Json(new { success = true, html = htmlCode });
        }

        public ActionResult AdList(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(db.Ads.ToList().Where(entity => entity.Search.ID == id).Take(50));
        }

        // POST: /Search/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "ID,Url,KeyWord")] Search search)
        {
            var currentUser = userManager.FindById(User.Identity.GetUserId());

            //Does user have already search
            IEnumerable<Search> searches = db.Searches.ToList().Where(entity => entity.User.Id == currentUser.Id);

            if (searches.Count() >= 1 && !Roles.IsUserInRole("admin") && !Roles.IsUserInRole("premium"))
                return Json(new { success = false, message = "Vous devez avoir un compte premium pour ajouter plus d'une recherche" });

            if (ModelState.IsValid)
            {
                search.Url = LBCMapping.HtmlParser.CleanCriteria(search.Url);
                search.KeyWord = LBCMapping.HtmlParser.ExtractKeyWordFromCriteria(search.Url);
                search.User = currentUser;
                db.Searches.Add(search);
                db.SaveChanges();

                log.Info("Add search #" + search.ID + " Url [" + search.Url + "] Keyword [" + search.KeyWord + "] by [" + search.User.UserName + "]");

                return Json(new { success = true, message = "ok" });
            }

            return Json(new { success = false, message = "Something bad..." });
        }

        // GET: /Search/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Search search = db.Searches.Find(id);
            if (search == null)
            {
                return HttpNotFound();
            }
            var currentUser = await userManager.FindByIdAsync(User.Identity.GetUserId());
            if (search.User.Id != currentUser.Id && currentUser.UserName != "admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            return View(search);
        }

        // POST: /Search/Edit/5
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Url,KeyWord")] Search search)
        {
            if (ModelState.IsValid)
            {
                db.Entry(search).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(search);
        }

        // GET: /Search/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Search search = db.Searches.Find(id);
            if (search == null)
            {
                return HttpNotFound();
            }
            var currentUser = await userManager.FindByIdAsync(User.Identity.GetUserId());
            if (search.User.Id != currentUser.Id && currentUser.UserName != "admin")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
            return View(search);
        }

        // POST: /Search/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Search search = await db.Searches.FindAsync(id);
            AspNetTimer.StopJob(search);
            db.Ads.RemoveRange(db.Ads.ToList().Where(entity => entity.Search.ID == search.ID));
            db.Searches.Remove(search);   
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
