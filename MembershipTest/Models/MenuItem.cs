using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MembershipTest.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string ConfigName { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string RscURL { get; set; }
        public string LtrURL { get; set; }
        public int ParentItem { get; set; }
    }
}
