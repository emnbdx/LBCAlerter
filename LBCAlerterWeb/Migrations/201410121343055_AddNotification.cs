namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notification",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Message = c.String(),
                        Important = c.Boolean(nullable: false),
                        Viewed = c.Boolean(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notification", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Notification", new[] { "User_Id" });
            DropTable("dbo.Notification");
        }
    }
}
