namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notification", "Date", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Notification", "Title", c => c.String(nullable: false));
            AlterColumn("dbo.Notification", "Message", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notification", "Message", c => c.String());
            AlterColumn("dbo.Notification", "Title", c => c.String());
            DropColumn("dbo.Notification", "Date");
        }
    }
}
