namespace Quotations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuotationUserField : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Quotations", name: "ApplicationUser_Id", newName: "User_Id");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Quotations", name: "User_Id", newName: "ApplicationUser_Id");
        }
    }
}
