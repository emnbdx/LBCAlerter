
namespace LBCAlerterWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    
    /// <summary>
    /// The application user.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the registration date.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Gets or sets the email verification token.
        /// </summary>
        public string EmailVerificationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is email verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the email reset token.
        /// </summary>
        public string EmailResetToken { get; set; }

        /// <summary>
        /// Gets or sets the email reset date.
        /// </summary>
        public DateTime? EmailResetDate { get; set; }

        /// <summary>
        /// Gets or sets the searches.
        /// </summary>
        public virtual ICollection<Search> Searches { get; set; }

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        public virtual ICollection<Notification> Notifications { get; set; }
    }

    /// <summary>
    /// The application db context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Gets or sets the searches.
        /// </summary>
        public DbSet<Search> Searches { get; set; }

        /// <summary>
        /// Gets or sets the attempts.
        /// </summary>
        public DbSet<Attempt> Attempts { get; set; }

        /// <summary>
        /// Gets or sets the ads.
        /// </summary>
        public DbSet<Ad> Ads { get; set; }

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        public System.Data.Entity.DbSet<LBCAlerterWeb.Models.Payment> Payments { get; set; }
    }

    /// <summary>
    /// The production initializer.
    /// </summary>
    public class ProductionInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        /// <summary>
        /// The seed.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager) { AllowOnlyAlphanumericUserNames = false };
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roles = { "admin", "premium", "user" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
            }

            context.SaveChanges();

            var user = new ApplicationUser { UserName = "eddy.montus@gmail.com", RegistrationDate = DateTime.Now };
            var adminresult = userManager.Create(user, "password");

            if (adminresult.Succeeded)
            {
                userManager.AddToRole(user.Id, "admin");
            }

            context.SaveChanges();

            base.Seed(context);
        }
    }

    /// <summary>
    /// The debug initializer.
    /// </summary>
    public class DebugInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        /// <summary>
        /// The seed.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager) { AllowOnlyAlphanumericUserNames = false };
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roles = { "admin", "premium", "user" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
            }

            string[] names = { "admin", "eddy.montus@gmail.com" };

            foreach (var name in names)
            {
                // Create User=Admin with password=123456
                var user = new ApplicationUser { UserName = name, RegistrationDate = DateTime.Now };
                var adminresult = userManager.Create(user, "123456");

                // Add User Admin to Role Admin
                if (adminresult.Succeeded)
                {
                    userManager.AddToRole(user.Id, name == "admin" ? "admin" : "user");
                }
            }

            base.Seed(context);
        }
    }
}