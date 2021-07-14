using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// this is essentially following https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio
// and just adding a new controller

namespace Tests.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class XmlNonValidatingInputController : ControllerBase
    {
        
        [HttpGet]
        public Int32 Get(String arg)
        {
            // bad input validation: no real validation, throws exception if xml is malformed or value is not a number
            // exception is usually not what we want for input validation as it is pretty expensive, and is also hard to
            // loalize for real users, and makes it a lot more difficult to validate multiple fields in parallel.
            // never the less this is typical application code:  construct an xml document, load with xml string, extract one or more values.
            // at the minimum, missing validation is telling an hacker that the application developer is lazy about quality,
            // and thus the hacker might continue to search for issuer.Worst case, some nodes are reused generically and
            // passed to some other interface of the application.

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(arg);
            int t = Int32.Parse(xd.SelectSingleNode("//test/value").InnerText);
            return t;
        }
    }
}
