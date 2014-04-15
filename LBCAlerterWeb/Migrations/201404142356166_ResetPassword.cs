namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResetPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "EmailResetToken", c => c.String());
            AddColumn("dbo.AspNetUsers", "EmailResetDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "EmailResetDate");
            DropColumn("dbo.AspNetUsers", "EmailResetToken");
        }
    }
}
