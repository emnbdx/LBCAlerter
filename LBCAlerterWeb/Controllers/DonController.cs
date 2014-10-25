
namespace LBCAlerterWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using LBCAlerterWeb.Models;

    using log4net;

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

            if (response != "VERIFIED")
            {
                return this.View();
            }

            var id = this.Request["txn_id"];
            var stringAmount = this.Request["mc_gross"];
            var currency = this.Request["mmc_currency"];
            var state = this.Request["payment_status"];
            var email = this.Request["payer_email"];

            var user = this.db.Users.FirstOrDefault(entry => entry.UserName == email);

            // validate the order
            decimal amount = 0;
            decimal.TryParse(stringAmount, out amount);

            var don = new Don
                          {
                              PaypalId = id,
                              CreationDate = DateTime.Now,
                              UpdateDate = DateTime.Now,
                              State = state,
                              Amount = amount,
                              Currency = currency,
                              User = user
                          };

            this.db.Dons.Add(don);
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
