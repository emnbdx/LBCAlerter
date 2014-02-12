using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data.Entity;

namespace LBCAlerterWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Search> Searches { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Search> Searches { get; set; }
        public DbSet<Ad> Ads { get; set; }
    }

    public class ProductionInitializer : IDatabaseInitializer<ApplicationDbContext>
    {
        public void InitializeDatabase(ApplicationDbContext context)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            string[] roles = { "admin", "premium", "user" };
            foreach (string role in roles)
            {
                if (!RoleManager.RoleExists(role))
                {
                    var roleresult = RoleManager.Create(new IdentityRole(role));
                }
            }

            var user = new ApplicationUser();
            user.UserName = "eddy.montus@gmail.com";
            var adminresult = UserManager.Create(user, "password");

            if (adminresult.Succeeded)
            {
                var result = UserManager.AddToRole(user.Id, "admin");
            }
        }
    }

    public class DebugInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
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