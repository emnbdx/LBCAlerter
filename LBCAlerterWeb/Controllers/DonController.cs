
namespace LBCAlerterWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LBCAlerterWeb.Models;

    using log4net;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using WebGrease;

    using LogManager = log4net.LogManager;

    /// <summary>
    /// The payment controller.
    /// </summary>
    [Authorize]
    public class DonController : Controller
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(DonController));

        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="DonController"/> class.
        /// </summary>
        public DonController()
        {
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.db));
            this.UserManager.UserValidator = new UserValidator<ApplicationUser>(this.UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        /// <summary>
        /// Gets or sets the user manager.
        /// </summary>
        private UserManager<ApplicationUser> UserManager { get; set; }

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
        /// The merci.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Merci()
        {
            return this.View();
        }

        /// <summary>
        /// The pay pal payment notification.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult PayPalPaymentNotification()
        {
            Log.Debug("New IPN notification...");
            
            // Receive IPN request from PayPal and parse all the variables returned
            var formVals = new Dictionary<string, string> { { "cmd", "_notify-validate" } };

            // if you want to use the PayPal sandbox change this from false to true
            var response = GetPayPalResponse(formVals, ConfigurationManager.AppSettings["paypalMode"] != "prod");

            var id = this.Request["txn_id"];
            var date = this.Request["payment_date"];
            var state = this.Request["payment_status"];
            var amount = this.Request["mc_gross"];
            var currency = this.Request["mc_currency"];
            var payerId = this.Request["payer_id"];
            var payerEmail = this.Request["payer_email"];
            var payerFirstName = this.Request["first_name"];
            var payerLastName = this.Request["last_name"];
            var custom = this.Request["custom"];
            
            var user = this.db.Users.FirstOrDefault(entry => entry.UserName == payerEmail)
                       ?? this.db.Users.FirstOrDefault(entry => entry.Id == custom);

            var realDate = ConvertPayPalDateTime(date);
            decimal realAmount;
            decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out realAmount);

            var don = new Don
                          {
                              PaypalId = id,
                              CreationDate = realDate,
                              UpdateDate = DateTime.Now,
                              State = response + "_" + state,
                              Amount = realAmount,
                              Currency = currency,
                              User = user,
                              PayerId = payerId,
                              PayerEmail = payerEmail,
                              PayerFirstName = payerFirstName,
                              PayerLastName = payerLastName,
                          };

            this.db.Dons.Add(don);

            if (user != null && !this.UserManager.IsInRole(user.Id, "premium") && realAmount > 0)
            {
                this.UserManager.AddToRole(user.Id, "premium");
            }

            this.db.SaveChanges();

            return this.View();
        }

        /// <summary>
        /// The get pay pal response.
        /// </summary>
        /// <param name="formVals">
        /// The form vals.
        /// </param>
        /// <param name="useSandbox">
        /// The use sandbox.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetPayPalResponse(Dictionary<string, string> formVals, bool useSandbox)
        {
            // Parse the variables
            // Choose whether to use sandbox or live environment
            var paypalUrl = useSandbox
                                   ? "https://www.sandbox.paypal.com/cgi-bin/webscr"
                                   : "https://www.paypal.com/cgi-bin/webscr";

            var req = (HttpWebRequest)WebRequest.Create(paypalUrl);

            // Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            var param = Request.BinaryRead(Request.ContentLength);
            var strRequest = Encoding.ASCII.GetString(param);

            var sb = new StringBuilder();
            sb.Append(strRequest);

            foreach (var key in formVals.Keys)
            {
                sb.AppendFormat("&{0}={1}", key, formVals[key]);
            }

            strRequest += sb.ToString();
            req.ContentLength = strRequest.Length;

            // for proxy
            // WebProxy proxy = new WebProxy(new Uri("http://urlort#");
            // req.Proxy = proxy;
            // Send the request to PayPal and get the response
            string response;
            using (var streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
            {
                streamOut.Write(strRequest);
                streamOut.Close();
                using (var streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    response = streamIn.ReadToEnd();
                }
            }

            return response;
        }

        /// <summary>
        /// The convert pay pal date time.
        /// </summary>
        /// <param name="payPalDateTime">
        /// The pay pal date time.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        private static DateTime ConvertPayPalDateTime(string payPalDateTime)
        {
            // Get the offset.
            // If C# supports switching on strings, it's probably more sensible to do that.
            int offset;
            if (payPalDateTime.EndsWith(" PDT"))
            {
                offset = 7;
            }
            else if (payPalDateTime.EndsWith(" PST"))
            {
                offset = 8;
            }
            else
            {
                throw new ArgumentException("La date n'est pas au bon format...");
            }

            // We've "parsed" the time zone, so remove it from the string.
            payPalDateTime = payPalDateTime.Substring(0, payPalDateTime.Length - 4);

            // Same formats as above, but with PST/PDT removed.
            string[] dateFormats = { "HH:mm:ss MMM dd, yyyy", "HH:mm:ss MMM. dd, yyyy" };

            // Parse the date. Throw an exception if it fails.
            DateTime ret = DateTime.ParseExact(
                payPalDateTime,
                dateFormats,
                new CultureInfo("en-US"),
                DateTimeStyles.None);

            // Add the offset, and make it a universal time.
            return ret.AddHours(offset);
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
