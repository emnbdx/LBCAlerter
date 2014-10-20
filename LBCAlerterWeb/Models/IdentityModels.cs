using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace LBCAlerterWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegistrationDate { get; set; }

        public string EmailVerificationToken { get; set; }

        public bool IsEmailVerified { get; set; }

        public string EmailResetToken { get; set; }

        public DateTime? EmailResetDate { get; set; }

        public virtual ICollection<Search> Searches { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema:false)
        {
        }

        public DbSet<Search> Searches { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }

    public class ProductionInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roles = { "admin", "premium", "user" };
            foreach (string role in roles)
            {
                if (!RoleManager.RoleExists(role))
                {
                    var roleresult = RoleManager.Create(new IdentityRole(role));
                }
            }
            context.SaveChanges();

            var user = new ApplicationUser();
            user.UserName = "eddy.montus@gmail.com";
            user.RegistrationDate = DateTime.Now;
            var adminresult = UserManager.Create(user, "password");

            if (adminresult.Succeeded)
            {
                var result = UserManager.AddToRole(user.Id, "admin");
            }
            context.SaveChanges();

            base.Seed(context);
        }
    }

    public class DebugInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roles = { "admin", "premium", "user" };
            foreach (string role in roles)
            {
                if (!RoleManager.RoleExists(role))
                {
                    var roleresult = RoleManager.Create(new IdentityRole(role));
                }
            }

            string[] names = { "admin", "eddy.montus@gmail.com" };
            string password = "123456";

            foreach (string name in names)
            {
                //Create User=Admin with password=123456
                var user = new ApplicationUser();
                user.UserName = name;
                user.RegistrationDate = DateTime.Now;
                var adminresult = UserManager.Create(user, password);

                //Add User Admin to Role Admin
                if (adminresult.Succeeded)
                {
                    var result = UserManager.AddToRole(user.Id, name == "admin" ? "admin" : "user");
                }
            }

            base.Seed(context);
        }
    }
}