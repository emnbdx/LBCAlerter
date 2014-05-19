using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using LBCAlerterWeb.Models;
using EMToolBox.Mail;
using System.Data.Entity;

namespace LBCAlerterWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationDbContext db;

        #region Advanced Registration
        private void SendEmailConfirmation(string to, string confirmationToken)
        {
            EMMail mail = new EMMail();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("[Title]", "Vous venez de vous inscrire sur LBCAlerter, MERCI !");
            parameters.Add("[Token]", confirmationToken);
            mail.Add("[LBCAlerter] - Confirmation de votre compte", to, "LBC_CONFIRMATION", parameters);
            //mail.SendSmtpMail("[LBCAlerter] - Confirmation de votre compte", to, MailPattern.GetPattern(MailType.Confirmation), parameters);
        }

        private bool ConfirmAccount(string confirmationToken)
        {
            ApplicationUser user = db.Users.SingleOrDefault(entry => entry.EmailVerificationToken == confirmationToken);
            if (user != null)
            {
                user.IsEmailVerified = true;
                db.SaveChanges();

                return true;
            }
            return false;
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    RegistrationDate = DateTime.Now,
                    EmailVerificationToken = Guid.NewGuid().ToString(),
                    IsEmailVerified = false
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "user");
                    SendEmailConfirmation(user.UserName, user.EmailVerificationToken);
                    return RedirectToAction("RegisterStepTwo");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> RegisterConfirmation(string Id)
        {
            if (ConfirmAccount(Id))
            {
                ApplicationUser user = db.Users.SingleOrDefault(entry => entry.EmailVerificationToken == Id && entry.IsEmailVerified == true);
                await SignInAsync(user, isPersistent: false);

                return RedirectToAction("Success", new { id = "Register" });
            }
            return RedirectToAction("Failure", new { id = "Register" });
        }
        #endregion

        #region Password Reset
        private void SendEmailReset(string to, string confirmationToken)
        {
            EMMail mail = new EMMail();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("[Title]", "Réinitialision de mot de passe");
            parameters.Add("[Token]", confirmationToken);
            mail.Add("[LBCAlerter] - réinitialision de mot de passe", to, "LBC_RESET", parameters);
            //mail.SendSmtpMail("[LBCAlerter] - réinitialision de mot de passe", to, MailPattern.GetPattern(MailType.Reset), parameters);
        }

        private string GeneratePasswordResetToken(string userName)
        {
            string token = Guid.NewGuid().ToString();

            ApplicationUser user = db.Users.FirstOrDefault(entry => entry.UserName == userName);
            if (user == null)
                return null;
            user.EmailResetToken = token;
            user.EmailResetDate = DateTime.Now;
            db.SaveChanges();
            
            return token;
        }

        private bool ResetPassword(ApplicationUser user, string newPassword)
        {
            //We have to remove the password before we can add it.
            IdentityResult result = UserManager.RemovePassword(user.Id);
            if (!result.Succeeded)
                return false;
            //We have to add it because we do not have the old password to change it.
            result = UserManager.AddPassword(user.Id, newPassword);
            if (!result.Succeeded)
                return false;

            //Lets remove the token so it cannot be used again.
            user.EmailResetToken = null;
            db.SaveChanges();
            return true;
        }

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                //L'utilisateur existe ?
                ApplicationUser user = db.Users.FirstOrDefault(entry => entry.UserName == model.Email);
                if (user == null)
                    ModelState.AddModelError("", "Votre email n'existe pas.");
                else
                {
                    string confirmationToken = GeneratePasswordResetToken(model.Email);
                    if(String.IsNullOrEmpty(confirmationToken))
                        ModelState.AddModelError("", "Votre email n'existe pas.");
                    else
                    {
                        SendEmailReset(model.Email, confirmationToken);
                        return RedirectToAction("ResetPasswordStepTwo");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Email invalide.");
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ResetPasswordConfirmation(ResetPasswordConfirmModel model)
        {
            ApplicationUser user = db.Users.FirstOrDefault(entry => entry.EmailResetToken == model.Token);
            if (user != null && ResetPassword(user, model.NewPassword))
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToAction("Success", new { id = "Reset" });
            }
            return RedirectToAction("Failure", new { id = "Reset" });
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(string Id)
        {
            ResetPasswordConfirmModel model = new ResetPasswordConfirmModel() { Token = Id };
            return View(model);
        }
        #endregion

        #region Shared Screen
        public ActionResult Success(string Id)
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Failure()
        {
            return View();
        }
        #endregion

        public AccountController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        [Authorize(Roles = "admin")]
        public async Task<ActionResult> All()
        {
            return View(await db.Users.OrderByDescending(entry => entry.RegistrationDate).ToListAsync());
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.Email, model.Password);
                if (user != null)
                {
                    if (user.IsEmailVerified)
                    {
                        await SignInAsync(user, model.RememberMe);
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Votre compte n'est pas validé, veuillez vérifier vos emails.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email ou mot de passe invalide.");
                }
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été modifié."
                : message == ManageMessageId.SetPasswordSuccess ? "Votre mot de passe a été défini."
                : message == ManageMessageId.RemoveLoginSuccess ? "La connexion externe a été supprimée."
                : message == ManageMessageId.Error ? "Une erreur s'est produite."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // Si nous sommes arrivés là, un échec s’est produit. Réafficher le formulaire
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Applications auxiliaires
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}