using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MembershipTest.Models
{
    public class MembershipUser
    {
        public int Id { get; set; }
        public string UserLoginId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SymmetricKey { get; set; }
    }
}
