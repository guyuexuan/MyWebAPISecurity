using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;
using MembershipTest.Models;

namespace MembershipTest.SecurityContext.Mapping
{
    public class MembershipUserMap : EntityTypeConfiguration<MembershipUser>
    {
        public MembershipUserMap()
        {
            HasKey(o => o.Id);

            Property(o => o.UserName)
                .HasColumnType("nvarchar")
                .HasMaxLength(15);

        }
    }
}
