namespace SlotAddiction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateSlotModelTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SlotModel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SlotModelName = c.String(),
                        SlotModelShortName = c.String(),
                        ThroughType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SlotModel");
        }
    }
}
