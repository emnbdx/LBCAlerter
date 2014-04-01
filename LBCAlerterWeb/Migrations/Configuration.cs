namespace LBCAlerterWeb.Migrations
{
    using LBCAlerterWeb.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<LBCAlerterWeb.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "LBCAlerterWeb.Models.ApplicationDbContext";
        }

        protected override void Seed(LBCAlerterWeb.Models.ApplicationDbContext context)
        {
            //Set existing user verified
            foreach(ApplicationUser u in context.Users)
            {
                u.EmailVerificationToken = Guid.NewGuid().ToString();
                u.IsEmailVerified = true;
            }

            context.SaveChanges();
        }
    }
}
