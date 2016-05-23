using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MembershipTest.SecurityContext;
using MembershipTest.Models;

namespace DataTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SecurityContext context = new SecurityContext();

            var m = new MembershipUser
            {
                Id = 1,
                UserLoginId = "21140211076",
                UserName = "hanlin",
                Password = "hanlin",
            };
            context.MembershipUser.Add(m);

            //var m = new MembershipUser
            //{
            //    Id = 1,
            //    UserLoginId = "21140211077",
            //    UserName = "larry",
            //    Password = "larry",
            //};
            context.MembershipUser.Add(m);

            context.SaveChanges();
        }
    }
}
