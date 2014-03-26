namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSearchInfos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Search", "CreationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Search", "MailAlert", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Search", "MailRecap", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Search", "LastRecap", c => c.DateTime());
            AddColumn("dbo.Search", "RefreshTime", c => c.Int(nullable: false, defaultValue: 15));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Search", "RefreshTime");
            DropColumn("dbo.Search", "LastRecap");
            DropColumn("dbo.Search", "MailRecap");
            DropColumn("dbo.Search", "MailAlert");
            DropColumn("dbo.Search", "CreationDate");
        }
    }
}
