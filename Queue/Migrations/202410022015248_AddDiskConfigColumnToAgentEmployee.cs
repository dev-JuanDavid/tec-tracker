namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDiskConfigColumnToAgentEmployee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Agent_Employee", "DiskConfig", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Agent_Employee", "DiskConfig");
        }
    }
}
