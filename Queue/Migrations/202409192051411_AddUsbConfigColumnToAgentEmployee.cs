namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUsbConfigColumnToAgentEmployee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Agent_Employee", "UsbConfig", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Agent_Employee", "UsbConfig");
        }
    }
}
