using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using MembershipTest.Models;
using MembershipTest.SecurityContext.Mapping;

namespace MembershipTest.SecurityContext
{
    public class SecurityContext : DbContext
    {
        public SecurityContext() : base("name=SecurityContext")
        { }

        public virtual DbSet<MembershipUser> MembershipUser { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new MembershipUserMap());
        }
    }
}
