using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SecurityTest.Models;

namespace SecurityTest.Controllers
{
    public class ResourceValuesController : ApiController
    {
        [Authorize(Roles = "admin")]
        public ResourceValues GetResourceValues()
        {
            return new ResourceValues
            {
                IDValue = "21140211076",
                NameValue = "韩林"
            };
        }
    }
}