namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CreationDate_To_Agent_NotificacionUsuario : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Agent_NotificacionUsuario", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Agent_NotificacionUsuario", "Date");
        }
    }
}
