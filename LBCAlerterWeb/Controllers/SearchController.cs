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
using System.ServiceModel.Syndication;

namespace LBCAlerterWeb.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private static ILog log = LogManager.GetLogger(typeof(SearchController));

        private ApplicationDbContext db;
        private UserManager<ApplicationUser> UserManager { get; set; }

        public SearchController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }       

        // GET: /Search/
        public async Task<ActionResult> Index()
        {
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            return View(db.Searches.Where(search => search.User.Id == currentUser.Id).ToList());
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
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(db.Ads.Where(ad => ad.Search.ID == id).OrderByDescending(ad => ad.Date).Take(50).ToList());
        }

        public ActionResult AdListFeed(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var search = db.Searches.FirstOrDefault(entry => entry.ID == id);
            var postItems = db.Ads.Where(ad => ad.Search.ID == id).OrderByDescending(ad => ad.Date).Take(50).ToList()
                .Select(p => new SyndicationItem(p.Title, "", new Uri(p.Url)));

            var feed = new SyndicationFeed(search.KeyWord, "", new Uri(LBCMapping.HtmlParser.URL_BASE + search.Url), postItems)
            {
                Copyright = new TextSyndicationContent("© " + DateTime.Today.Year + " - LBCAlerter"),
                Language = "fr"
            };

            return new RssActionResult(new Rss20FeedFormatter(feed));
        }

        // POST: /Search/Create
        // Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        // plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "Url,RefreshTime,MailAlert,MailRecap")] Search search)
        {
            var currentUser = UserManager.FindById(User.Identity.GetUserId());

            //Does user have already search
            IEnumerable<Search> searches = db.Searches.Where(entry => entry.User.Id == currentUser.Id).ToList();

            if (searches.Count() >= 5 && !Roles.IsUserInRole("admin") && !Roles.IsUserInRole("premium"))
                return Json(new { success = false, message = "Vous devez avoir un compte premium pour ajouter plus de cinq recherches" });

            if (ModelState.IsValid)
            {
                search.Url = LBCMapping.HtmlParser.CleanCriteria(search.Url);
                search.CreationDate = DateTime.Now;
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
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
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
        public async Task<ActionResult> Edit([Bind(Include = "ID,Url,KeyWord,MailAlert,MailRecap,RefreshTime,CreationDate,LastRecap")] Search search)
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
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
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
            db.Ads.RemoveRange(db.Ads.Where(entry => entry.Search.ID == search.ID).ToList());
            db.Attempts.RemoveRange(db.Attempts.Where(entry => entry.Search.ID == search.ID).ToList());
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
