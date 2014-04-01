namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccountConfirmation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "EmailVerificationToken", c => c.String());
            AddColumn("dbo.AspNetUsers", "IsEmailVerified", c => c.Boolean());
        }
        
        public override void Down()
        {
            AddColumn("dbo.Ad", "PhoneUrl", c => c.String());
            AddColumn("dbo.Ad", "EmailUrl", c => c.String());
        }
    }
}
