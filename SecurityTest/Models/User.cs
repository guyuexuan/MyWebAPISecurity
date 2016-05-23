using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecurityTest.Models
{
    public class User
    {
        public virtual string UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string UserRole { get; set; }
    }
}