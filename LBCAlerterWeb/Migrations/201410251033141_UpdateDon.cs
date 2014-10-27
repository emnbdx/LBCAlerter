namespace LBCAlerterWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Don", "PayerId", c => c.String());
            AddColumn("dbo.Don", "PayerEmail", c => c.String());
            AddColumn("dbo.Don", "PayerFirstName", c => c.String());
            AddColumn("dbo.Don", "PayerLastName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Don", "PayerLastName");
            DropColumn("dbo.Don", "PayerFirstName");
            DropColumn("dbo.Don", "PayerEmail");
            DropColumn("dbo.Don", "PayerId");
        }
    }
}
