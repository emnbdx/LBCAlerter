namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDon : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Don",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PaypalId = c.String(),
                        CreationDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        State = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Don", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Don", new[] { "User_Id" });
            DropTable("dbo.Don");
        }
    }
}
