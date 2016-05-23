namespace MembershipTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MembershipUsers", "UserLoginId", c => c.String());
            DropColumn("dbo.MembershipUsers", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MembershipUsers", "UserId", c => c.String());
            DropColumn("dbo.MembershipUsers", "UserLoginId");
        }
    }
}
