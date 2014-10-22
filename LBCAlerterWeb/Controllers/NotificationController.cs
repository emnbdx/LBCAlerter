
namespace LBCAlerterWeb.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LBCAlerterWeb.Models;
    using Microsoft.AspNet.Identity;

    /// <summary>
    /// The notification controller.
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// GET: Notifications
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            return this.View(await this.db.Notifications.Where(notif => notif.User.Id == userId).OrderByDescending(notif => notif.Date).ToListAsync());
        }

        /// <summary>
        /// GET: All Notifications
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> All()
        {
            return this.View(await this.db.Notifications.OrderByDescending(notif => notif.Date).ToListAsync());
        }

        /// <summary>
        /// GET: Notifications/Create
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public ActionResult Create()
        {
            return this.View();
        }

        /// <summary>
        /// POST: Notifications/Create
        /// Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        /// plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="notification">
        /// The notification.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Create([Bind(Include = "Title,Message,Date,Important,Viewed")] Notification notification)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(notification);
            }

            foreach (var applicationUser in this.db.Users)
            {
                this.db.Notifications.Add(
                    new Notification
                        {
                            Title = notification.Title,
                            Message = notification.Message,
                            Date = notification.Date,
                            Important = notification.Important,
                            Viewed = notification.Viewed,
                            User = applicationUser
                        });
            }

            await this.db.SaveChangesAsync();
            return this.RedirectToAction("All");
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
