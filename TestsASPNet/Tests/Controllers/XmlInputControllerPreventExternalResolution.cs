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
    public class ExternalEntityEncountered : NotImplementedException { }

    [ApiController]
    [Route("[controller]")]
    public class XmlInputControllerPreventExternalResolution : ControllerBase
    {
        //
        // use https://www.urlencoder.io/ to encode XML to URL encoded  (if necessary, often works without encoding)
        // <test><value>3</value></test> => %3Ctest%3E%3Cvalue%3E3%3C%2Fvalue%3E%3C%2Ftest%3E
        // => /xmlinput?arg=%3Ctest%3E%3Cvalue%3E3%3C%2Fvalue%3E%3C%2Ftest%3E
        [HttpGet]
        public Int32 Get(String arg)
        {
            // bad input validation: no real validation, throws exception if xml is malformed or value is not a number
            // exception is usually not what we want for input validation as it is pretty expensive, is also hard to
            // loalize for real users, and makes it a lot more difficult to validate multiple fields in parallel.
            XmlDocument xd = new XmlDocument();
            xd.XmlResolver = new PreventExternalResolution();
            xd.LoadXml(arg);
            if (false) // Validate is only for XSDs
                xd.Validate((e, n) =>
                {
                    BadRequest();
                });
            int t = Int32.Parse(xd.SelectSingleNode("//test/value").InnerText);
            return t;
        }

        private class PreventExternalResolution : XmlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                throw new ExternalEntityEncountered();
            }
        }
    }
}
