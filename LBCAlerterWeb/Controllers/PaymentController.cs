
namespace LBCAlerterWeb.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LBCAlerterWeb.Models;

    using PayPal;
    using PayPal.Api.Payments;

    using Payment = LBCAlerterWeb.Models.Payment;

    /// <summary>
    /// The payment controller.
    /// </summary>
    [Authorize]
    public class PaymentController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// The token credential.
        /// </summary>
        private readonly OAuthTokenCredential tokenCredential;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentController"/> class.
        /// </summary>
        public PaymentController()
        {
            var config = PayPal.Manager.ConfigManager.Instance.GetProperties();
            var clientId = config[BaseConstants.ClientId];
            var clientSecret = config[BaseConstants.ClientSecret];

            this.tokenCredential = new OAuthTokenCredential(clientId, clientSecret);
        }

        /// <summary>
        /// GET: Payment
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Index()
        {
            return this.View(await this.db.Payments.ToListAsync());
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

            var payment = await this.db.Payments.FindAsync(id);
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
            var accessToken = this.tokenCredential.GetAccessToken();

            var billingAddress = new Address
                                     {
                                         line1 = "52 N Main ST",
                                         city = "Johnstown",
                                         country_code = "US",
                                         postal_code = "43210",
                                         state = "OH"
                                     };

            var creditCard = new CreditCard
                                 {
                                     number = "4417119669820331",
                                     type = "visa",
                                     expire_month = 11,
                                     expire_year = 2018,
                                     cvv2 = 874,
                                     first_name = "Joe",
                                     last_name = "Shopper",
                                     billing_address = billingAddress
                                 };

            var amountDetails = new Details { subtotal = "7.41", tax = "0.03", shipping = "0.03" };

            var amount = new Amount { total = "7.47", currency = "USD", details = amountDetails };

            var transaction = new Transaction
                                  {
                                      amount = amount,
                                      description = "This is the payment transaction description."
                                  };

            var transactions = new List<Transaction> { transaction };

            var fundingInstrument = new FundingInstrument { credit_card = creditCard };

            var fundingInstruments = new List<FundingInstrument> { fundingInstrument };

            var payer = new Payer { funding_instruments = fundingInstruments, payment_method = "credit_card" };

            var payment = new PayPal.Api.Payments.Payment
                              {
                                  intent = "sale",
                                  payer = payer,
                                  transactions = transactions
                              };

            var createdPayment = payment.Create(accessToken);

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
        public async Task<ActionResult> Create([Bind(Include = "ID,PaypalId,CreationDate,UpdateDate,State,Amount,Currency")] Payment payment)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(payment);
            }

            this.db.Payments.Add(payment);
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
