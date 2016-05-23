namespace MembershipTest.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using MembershipTest.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<MembershipTest.SecurityContext.SecurityContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MembershipTest.SecurityContext.SecurityContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            //context.MembershipUser.AddOrUpdate(
            //    new MembershipUser {
            //        Id = 1,
            //        UserId = "21140211076",
            //        UserName = "hanlin",
            //        Password = "hanlin",
            //    }
            //    );
            //context.SaveChanges();
        }
    }
}
