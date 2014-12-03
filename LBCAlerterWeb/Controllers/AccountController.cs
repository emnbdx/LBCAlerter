namespace LBCAlerterWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using EMToolBox.Mail;
    using LBCAlerterWeb.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.Owin.Security;

    /// <summary>
    /// The account controller.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        #region Advanced Registration

        /// <summary>
        /// The send email confirmation.
        /// </summary>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        private void SendEmailConfirmation(string to, string confirmationToken)
        {
            var mail = new EmMail();
            var parameters = @" {
                                    'Title': 'Vous venez de vous inscrire sur LBCAlerter, MERCI !',
                                    'Token': '" + confirmationToken + @"'
                                }";

            mail.Add("[LBCAlerter] - Confirmation de votre compte", to, "LBC_CONFIRMATION", parameters);
        }

        /// <summary>
        /// The confirm account.
        /// </summary>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ConfirmAccount(string confirmationToken)
        {
            var user = this.db.Users.SingleOrDefault(entry => entry.EmailVerificationToken == confirmationToken);
            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            this.db.SaveChanges();

            return true;
        }

        /// <summary>
        /// GET: /Account/Register
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var randomUser = this.db.Users.FirstOrDefault();

            var user = new ApplicationUser
                           {
                               UserName = model.Email,
                               RegistrationDate = DateTime.Now,
                               EmailVerificationToken = Guid.NewGuid().ToString(),
                               IsEmailVerified = false
                           };
            var result = await this.UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                this.UserManager.AddToRole(user.Id, "user");
                this.SendEmailConfirmation(user.UserName, user.EmailVerificationToken);

                // Add notification 
                if (randomUser == null)
                {
                    return this.RedirectToAction("RegisterStepTwo");
                }

                var importantRecorded = false;
                foreach (var notification in randomUser.Notifications)
                {
                    this.db.Notifications.Add(
                        new Notification
                            {
                                Title = notification.Title,
                                Message = notification.Message,
                                Date = notification.Date,
                                Important = !importantRecorded && notification.Important,
                                Viewed = false,
                                User = user
                            });

                    if (notification.Important)
                    {
                        importantRecorded = true;
                    }
                }

                await this.db.SaveChangesAsync();

                return this.RedirectToAction("RegisterStepTwo");
            }

            this.AddErrors(result);

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return this.View(model);
        }

        /// <summary>
        /// The register step two.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return this.View();
        }

        /// <summary>
        /// The register confirmation.
        /// </summary>
        /// <param name="Id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [AllowAnonymous]
        public async Task<ActionResult> RegisterConfirmation(string Id)
        {
            if (!this.ConfirmAccount(Id))
            {
                return this.RedirectToAction("Failure", new { id = "Register" });
            }

            var user = this.db.Users.SingleOrDefault(entry => entry.EmailVerificationToken == Id && entry.IsEmailVerified == true);
            await this.SignInAsync(user, isPersistent: false);

            return this.RedirectToAction("Success", new { id = "Register" });
        }
        #endregion

        #region Password Reset

        /// <summary>
        /// The send email reset.
        /// </summary>
        /// <param name="to">
        /// The to.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        private void SendEmailReset(string to, string confirmationToken)
        {
            var mail = new EmMail();
            var parameters = @" {
                                    'Title': 'Réinitialision de mot de passe',
                                    'Token': '" + confirmationToken + @"'
                                }";

            mail.Add("[LBCAlerter] - réinitialision de mot de passe", to, "LBC_RESET", parameters);
        }

        /// <summary>
        /// The generate password reset token.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GeneratePasswordResetToken(string userName)
        {
            var token = Guid.NewGuid().ToString();

            var user = this.db.Users.FirstOrDefault(entry => entry.UserName == userName);
            if (user == null)
            {
                return null;
            }

            user.EmailResetToken = token;
            user.EmailResetDate = DateTime.Now;
            this.db.SaveChanges();
            
            return token;
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="newPassword">
        /// The new password.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ResetPassword(ApplicationUser user, string newPassword)
        {
            // We have to remove the password before we can add it.
            var result = this.UserManager.RemovePassword(user.Id);
            if (!result.Succeeded)
            {
                return false;
            }

            // We have to add it because we do not have the old password to change it.
            result = this.UserManager.AddPassword(user.Id, newPassword);
            if (!result.Succeeded)
            {
                return false;
            }

            // Lets remove the token so it cannot be used again.
            user.EmailResetToken = null;
            this.db.SaveChanges();
            return true;
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return this.View();
        }

        /// <summary>
        /// The reset password.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                // L'utilisateur existe ?
                var user = this.db.Users.FirstOrDefault(entry => entry.UserName == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Votre email n'existe pas.");
                }
                else
                {
                    var confirmationToken = this.GeneratePasswordResetToken(model.Email);
                    if (string.IsNullOrEmpty(confirmationToken))
                    {
                        ModelState.AddModelError(string.Empty, "Votre email n'existe pas.");
                    }
                    else
                    {
                        this.SendEmailReset(model.Email, confirmationToken);
                        return this.RedirectToAction("ResetPasswordStepTwo");
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email invalide.");
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return this.View(model);
        }

        /// <summary>
        /// The reset password step two.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordStepTwo()
        {
            return this.View();
        }

        /// <summary>
        /// The reset password confirmation.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ResetPasswordConfirmation(ResetPasswordConfirmModel model)
        {
            var user = this.db.Users.FirstOrDefault(entry => entry.EmailResetToken == model.Token);
            if (user == null || !this.ResetPassword(user, model.NewPassword))
            {
                return this.RedirectToAction("Failure", new { id = "Reset" });
            }

            await this.SignInAsync(user, isPersistent: false);
            return this.RedirectToAction("Success", new { id = "Reset" });
        }

        /// <summary>
        /// The reset password confirmation.
        /// </summary>
        /// <param name="Id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(string Id)
        {
            var model = new ResetPasswordConfirmModel() { Token = Id };
            return this.View(model);
        }
        #endregion

        #region Shared Screen

        /// <summary>
        /// The success.
        /// </summary>
        /// <param name="Id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Success(string Id)
        {
            return this.View();
        }

        /// <summary>
        /// The failure.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult Failure()
        {
            return this.View();
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController()
        {
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.db));
            this.UserManager.UserValidator = new UserValidator<ApplicationUser>(this.UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        public UserManager<ApplicationUser> UserManager { get; private set; }

        /// <summary>
        /// The all.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> All()
        {
            return this.View(await this.db.Users.OrderByDescending(entry => entry.RegistrationDate).ToListAsync());
        }

        /// <summary>
        /// The add to premium.
        /// </summary>
        /// <param name="id">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddToPremium(string id)
        {
            var user = await this.db.Users.FirstOrDefaultAsync(entry => entry.Id == id);

            if (user != null && !this.UserManager.IsInRole(user.Id, "premium"))
            {
                this.UserManager.AddToRole(user.Id, "premium");
            }

            return this.RedirectToAction("All");
        }

        /// <summary>
        /// GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.UserManager.FindAsync(model.Email, model.Password);
            if (user != null)
            {
                if (user.IsEmailVerified)
                {
                    await this.SignInAsync(user, model.RememberMe);
                    return this.RedirectToLocal(returnUrl);
                }

                this.ModelState.AddModelError(string.Empty, "Votre compte n'est pas validé, veuillez vérifier vos emails.");
            }
            else
            {
                this.ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return this.View(model);
        }

        /// <summary>
        /// POST: /Account/Disassociate
        /// </summary>
        /// <param name="loginProvider">
        /// The login provider.
        /// </param>
        /// <param name="providerKey">
        /// The provider key.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            var result = await this.UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            message = result.Succeeded ? ManageMessageId.RemoveLoginSuccess : ManageMessageId.Error;

            return this.RedirectToAction("Manage", new { Message = message });
        }

        /// <summary>
        /// GET: /Account/Manage
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été modifié."
                : message == ManageMessageId.SetPasswordSuccess ? "Votre mot de passe a été défini."
                : message == ManageMessageId.RemoveLoginSuccess ? "La connexion externe a été supprimée."
                : message == ManageMessageId.Error ? "Une erreur s'est produite."
                : string.Empty;
            ViewBag.HasLocalPassword = this.HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Manage
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            var hasPassword = this.HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (!this.ModelState.IsValid)
                {
                    return this.View(model);
                }

                var result = await this.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                }

                this.AddErrors(result);
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                var state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (!this.ModelState.IsValid)
                {
                    return this.View(model);
                }

                var result = await this.UserManager.AddPasswordAsync(this.User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    return this.RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                }

                this.AddErrors(result);
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return this.View(model);
        }

        /// <summary>
        /// POST: /Account/LogOff
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut();
            return this.RedirectToAction("Index", "Search");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.UserManager != null)
            {
                this.UserManager.Dispose();
                this.UserManager = null;
            }

            base.Dispose(disposing);
        }

        #region Applications auxiliaires

        /// <summary>
        /// Gets the authentication manager.
        /// </summary>
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// The sign in async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="isPersistent">
        /// The is persistent.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await this.UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            this.AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        /// <summary>
        /// The add errors.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        /// <summary>
        /// The has password.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool HasPassword()
        {
            var user = this.UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        /// <summary>
        /// The manage message id.
        /// </summary>
        public enum ManageMessageId
        {
            /// <summary>
            /// The change password success.
            /// </summary>
            ChangePasswordSuccess,

            /// <summary>
            /// The set password success.
            /// </summary>
            SetPasswordSuccess,

            /// <summary>
            /// The remove login success.
            /// </summary>
            RemoveLoginSuccess,

            /// <summary>
            /// The error.
            /// </summary>
            Error
        }

        /// <summary>
        /// The redirect to local.
        /// </summary>
        /// <param name="returnUrl">
        /// The return url.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            
            return this.RedirectToAction("Index", "Search");
        }

        #endregion
    }
}