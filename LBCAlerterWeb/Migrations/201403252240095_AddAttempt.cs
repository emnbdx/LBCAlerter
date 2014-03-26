namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAttempt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attempts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ProcessDate = c.DateTime(nullable: false),
                        AdsFound = c.Int(nullable: false),
                        Search_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Search", t => t.Search_ID)
                .Index(t => t.Search_ID);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attempts", "Search_ID", "dbo.Search");
            DropIndex("dbo.Attempts", new[] { "Search_ID" });
            DropTable("dbo.Attempts");
        }
    }
}
