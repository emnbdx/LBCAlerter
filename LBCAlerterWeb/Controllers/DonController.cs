
namespace LBCAlerterWeb.Controllers
{
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LBCAlerterWeb.Models;

    /// <summary>
    /// The payment controller.
    /// </summary>
    [Authorize]
    public class DonController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// GET: Payment
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Index()
        {
            return this.View(await this.db.Dons.ToListAsync());
        }

        /// <summary>
        /// GET: Payment/Details/5
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var payment = await this.db.Dons.FindAsync(id);
            if (payment == null)
            {
                return this.HttpNotFound();
            }

            return this.View(payment);
        }

        /// <summary>
        /// GET: Payment/Create
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            return this.View();
        }
        
        /// <summary>
        /// POST: Payment/Create
        /// Afin de déjouer les attaques par sur-validation, activez les propriétés spécifiques que vous voulez lier. Pour 
        /// plus de détails, voir  http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="payment">
        /// The payment.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,PaypalId,CreationDate,UpdateDate,State,Amount,Currency")] Don payment)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(payment);
            }

            this.db.Dons.Add(payment);
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
