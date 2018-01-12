namespace MazeApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVisitedLocationsString : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Players", "VisitedLocations", c => c.String());
            AddColumn("dbo.Players", "Inventory", c => c.String());
            DropColumn("dbo.Players", "VisitedEntrance");
            DropColumn("dbo.Players", "VisitedLeftFork");
            DropColumn("dbo.Players", "VisitedRightFork");
            DropColumn("dbo.Players", "VisitedHill");
            DropColumn("dbo.Players", "VisitedValley");
            DropColumn("dbo.Players", "HowManyRightForkSteps");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Players", "HowManyRightForkSteps", c => c.Int(nullable: false));
            AddColumn("dbo.Players", "VisitedValley", c => c.Boolean(nullable: false));
            AddColumn("dbo.Players", "VisitedHill", c => c.Boolean(nullable: false));
            AddColumn("dbo.Players", "VisitedRightFork", c => c.Boolean(nullable: false));
            AddColumn("dbo.Players", "VisitedLeftFork", c => c.Boolean(nullable: false));
            AddColumn("dbo.Players", "VisitedEntrance", c => c.Boolean(nullable: false));
            DropColumn("dbo.Players", "Inventory");
            DropColumn("dbo.Players", "VisitedLocations");
        }
    }
}
