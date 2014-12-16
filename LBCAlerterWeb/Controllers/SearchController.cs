namespace LBCAlerterWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Syndication;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    using LBCAlerterWeb.Models;

    using LBCMapping;

    using log4net;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    /// <summary>
    /// The search controller.
    /// </summary>
    [Authorize]
    public class SearchController : Controller
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(SearchController));

        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchController"/> class.
        /// </summary>
        public SearchController()
        {
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.db));
            this.UserManager.UserValidator = new UserValidator<ApplicationUser>(this.UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        /// <summary>
        /// Gets or sets the user manager.
        /// </summary>
        private UserManager<ApplicationUser> UserManager { get; set; }

        /// <summary>
        /// GET: /Search/
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            return this.View(this.db.Searches.Where(search => search.User.Id == userId));
        }

        /// <summary>
        /// The all.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> All()
        {
            return this.View(await this.db.Searches.ToListAsync());
        }

        /// <summary>
        /// GET: /Search/Create
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            return this.View(this.db.Searches.Where(search => search.User.Id == userId));
        }

        /// <summary>
        /// The create step two.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult CreateStepTwo(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Url = url;

            var userId = User.Identity.GetUserId();
            return this.View(this.db.Searches.Where(search => search.User.Id == userId));
        }

        /// <summary>
        /// The ad list.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult AdList(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return this.View(this.db.Searches.FirstOrDefault(search => search.ID == id));
        }

        /// <summary>
        /// The ad list feed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult AdListFeed(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var search = this.db.Searches.First(entry => entry.ID == id);
            var postItems = this.db.Ads.Where(ad => ad.Search.ID == id).OrderByDescending(ad => ad.Date).Take(50).ToList()
                .Select(p => new SyndicationItem(p.Title, string.Empty, new Uri(p.Url)));

            var feed = new SyndicationFeed(search.KeyWord, string.Empty, new Uri(HtmlParser.UrlBase + search.Url), postItems)
            {
                Copyright = new TextSyndicationContent("© " + DateTime.Today.Year + " - LBCAlerter"),
                Language = "fr"
            };

            return new RssActionResult(new Rss20FeedFormatter(feed));
        }

        /// <summary>
        /// POST: /Search/Create
        /// Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        /// plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="search">
        /// The search.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Create([Bind(Include = "Url,RefreshTime")] Search search)
        {
            var currentUser = this.UserManager.FindById(User.Identity.GetUserId());

            // Does user have already search
            IEnumerable<Search> searches = this.db.Searches.Where(entry => entry.User.Id == currentUser.Id).ToList();

            if (searches.Count() >= 5 && !Roles.IsUserInRole("admin") && !Roles.IsUserInRole("premium"))
            {
                return
                    this.Json(
                        new
                            {
                                success = false,
                                message = "Il vous faut un compte premium pour ajouter plus de 5 recherches.\r\nUn compte premium s'obtient en faisant un don du montant que vous voulez."
                            });
            }

            if (!this.ModelState.IsValid)
            {
                return this.Json(new { success = false, message = "Something bad..." });
            }

            search.Url = HtmlParser.CleanCriteria(search.Url);
            search.CreationDate = DateTime.Now;
            search.KeyWord = HtmlParser.ExtractKeyWordFromCriteria(search.Url);
            search.MailAlert = true;
            search.MailRecap = false;
            search.User = currentUser;

            if (!Roles.IsUserInRole("admin") && !Roles.IsUserInRole("premium"))
            {
                search.RefreshTime = 60;
            }
            else
            {
                search.RefreshTime = 15;
            }

            search.Enabled = true;

            this.db.Searches.Add(search);
            this.db.SaveChanges();

            Log.Info("Add search #" + search.ID + " Url [" + search.Url + "] Keyword [" + search.KeyWord + "] by [" + search.User.UserName + "]");

            return this.Json(new { success = true, message = "ok" });
        }

        /// <summary>
        /// GET: /Search/Edit/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var search = this.db.Searches.Find(id);
            if (search == null)
            {
                return this.HttpNotFound();
            }

            if (search.User.Id != User.Identity.GetUserId() && !Roles.IsUserInRole("admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            return this.View(search);
        }

        /// <summary>
        /// POST: /Search/Edit/5
        /// Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        /// plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="search">
        /// The search.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Url,KeyWord,MailAlert,MailRecap,RefreshTime,Enabled,CreationDate,LastRecap")] Search search)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(search);
            }

            if (!Roles.IsUserInRole("admin") && !Roles.IsUserInRole("premium") && search.RefreshTime < 60)
            {
                search.RefreshTime = 60;
            }

            this.db.Entry(search).State = EntityState.Modified;
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// GET: /Search/Disable?id=5&amp;adId=&
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="adId">
        /// The ad id
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public async Task<ActionResult> Disable(int? id, int? adId)
        {
            if (id == null || adId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var search = this.db.Searches.Find(id);
            if (search == null)
            {
                return this.HttpNotFound();
            }

            if (search.Ads.Any(entry => entry.ID == adId))
            {
                search.Enabled = false;
            }

            this.db.Entry(search).State = EntityState.Modified;
            await this.db.SaveChangesAsync();

            return this.View(search);
        }

        /// <summary>
        /// GET: /Search/Delete/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var search = this.db.Searches.Find(id);
            if (search == null)
            {
                return this.HttpNotFound();
            }

            if (search.User.Id != User.Identity.GetUserId() && !Roles.IsUserInRole("admin"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            return this.View(search);
        }

        /// <summary>
        /// POST: /Search/Delete/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var search = await this.db.Searches.FindAsync(id);
            this.db.Ads.RemoveRange(this.db.Ads.Where(entry => entry.Search.ID == search.ID).ToList());
            this.db.Attempts.RemoveRange(this.db.Attempts.Where(entry => entry.Search.ID == search.ID).ToList());
            this.db.Searches.Remove(search);
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
