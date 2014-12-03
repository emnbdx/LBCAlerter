namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdContent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdContent",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Value = c.String(),
                        Ad_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Ad", t => t.Ad_ID)
                .Index(t => t.Ad_ID);
            
            AddColumn("dbo.Ad", "Hash", c => c.String());
            AddColumn("dbo.Search", "Enabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.Ad", "PictureUrl");
            DropColumn("dbo.Ad", "Place");
            DropColumn("dbo.Ad", "Price");
            DropColumn("dbo.Ad", "Phone");
            DropColumn("dbo.Ad", "AllowCommercial");
            DropColumn("dbo.Ad", "Name");
            DropColumn("dbo.Ad", "ContactUrl");
            DropColumn("dbo.Ad", "Param");
            DropColumn("dbo.Ad", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Ad", "Description", c => c.String());
            AddColumn("dbo.Ad", "Param", c => c.String());
            AddColumn("dbo.Ad", "ContactUrl", c => c.String());
            AddColumn("dbo.Ad", "Name", c => c.String());
            AddColumn("dbo.Ad", "AllowCommercial", c => c.Boolean(nullable: false));
            AddColumn("dbo.Ad", "Phone", c => c.String());
            AddColumn("dbo.Ad", "Price", c => c.String());
            AddColumn("dbo.Ad", "Place", c => c.String());
            AddColumn("dbo.Ad", "PictureUrl", c => c.String());
            DropForeignKey("dbo.AdContent", "Ad_ID", "dbo.Ad");
            DropIndex("dbo.AdContent", new[] { "Ad_ID" });
            DropColumn("dbo.Search", "Enabled");
            DropColumn("dbo.Ad", "Hash");
            DropTable("dbo.AdContent");
        }
    }
}
