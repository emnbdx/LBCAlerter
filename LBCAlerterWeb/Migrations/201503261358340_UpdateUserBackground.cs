namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserBackground : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Background", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Background");
        }
    }
}
