namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FullAdValue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ad", "Phone", c => c.String());
            AddColumn("dbo.Ad", "AllowCommercial", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Ad", "Name", c => c.String());
            AddColumn("dbo.Ad", "ContactUrl", c => c.String());
            AddColumn("dbo.Ad", "Param", c => c.String());
            AddColumn("dbo.Ad", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ad", "Description");
            DropColumn("dbo.Ad", "Param");
            DropColumn("dbo.Ad", "ContactUrl");
            DropColumn("dbo.Ad", "Name");
            DropColumn("dbo.Ad", "AllowCommercial");
            DropColumn("dbo.Ad", "Phone");
        }
    }
}
