namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTypeColumnToAgentEmployeeUsb : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Agent_EmployeeUsb", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Agent_EmployeeUsb", "Type");
        }
    }
}
