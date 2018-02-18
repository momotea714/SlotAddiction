namespace SlotAddiction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTempoTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tempo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StoreURL = c.String(),
                        StoreName = c.String(),
                        SlotMachineStartNo = c.Int(nullable: false),
                        SlotMachineEndNo = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Tempo");
        }
    }
}
