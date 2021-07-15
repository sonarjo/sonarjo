using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// this is essentially following https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio
// and just adding a new controller

namespace Tests.Controllers
{
    [ApiController]
    [Route("[validatinexternalgcontroller]")]
    public class XmlValidatingWithExternalResolverController : ControllerBase
    {
        const String DefaultDtd = "<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'>";

        [HttpGet]
        public Single Get(String arg)
        {
            // this is not the best input validation possible, as we throw an exception early where probably we would continue to
            // validate more - or terminate with less overhead than an exception. but kiss for the sake of the example.
            // but the code below does really validate the XML corresponds to the DTDs provided.
            
            // as a bonus (and because many appliations don´t send the optional DOCTYPE whereas validation requires it),
            // the default DTD is added if missing.
            if (!arg.Contains("<!DOCTYPE"))
            {
                int insertpos = arg.StartsWith("<?xml") ? arg.IndexOf("?>") + 2 : 0;
                arg = arg.Substring(0, insertpos) + DefaultDtd + arg.Substring(insertpos);
            }
            // summary: an XmlReader with adequate XmlReaderSettings is required, plus a resolver that returns only predefined DTDs.
            // ValidationEventHandler is optional according to https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlreadersettings.validationeventhandler?view=net-5.0
            // XmlUrlResolver is unsafe !!!
            var settings = new XmlReaderSettings { ValidationType = ValidationType.DTD, DtdProcessing = DtdProcessing.Parse, XmlResolver = new XmlUrlResolver() };
            settings.ValidationEventHandler += (s,e) => {
                BadRequest(); // BadRequest does not stop processing.
                throw new ValidationFailedException(e.Message, e.Exception);
            };
            XmlDocument xd = new XmlDocument();
            var reader = XmlReader.Create(new StringReader(arg), settings);
            xd.Load(reader);
            Single f = Single.Parse(xd.SelectSingleNode("//book/price").InnerText);
            return f;
        }

    }
}
