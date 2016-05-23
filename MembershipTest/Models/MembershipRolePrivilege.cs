using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MembershipTest.Models
{
    public class MembershipRolePrivilege
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PrivilegeId { get; set; }
    }
}
