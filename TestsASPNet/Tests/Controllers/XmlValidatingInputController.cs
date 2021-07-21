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
    public class UnexpectedExternalEntityEncounteredException : XmlException { } // NOSONAR - serialization not required and keep this example simple
    public class ValidationFailedException : XmlException // NOSONAR - serialization not required and keep this example simple
    {
        public ValidationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    [ApiController]
    [Route("[validatingcontroller]")]
    public class XmlValidatingInputController : ControllerBase
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
#pragma warning disable RS0030 // Do not used banned APIs - we are using a seucre XmlResolver plus validating the document
            var settings = new XmlReaderSettings { ValidationType = ValidationType.DTD, DtdProcessing = DtdProcessing.Parse, XmlResolver = new LocalResolver() };
            settings.ValidationEventHandler += (s,e) => {
                BadRequest(); // BadRequest does not stop processing.
                throw new ValidationFailedException(e.Message, e.Exception);
            };
            XmlDocument xd = new XmlDocument();
            var reader = XmlReader.Create(new StringReader(arg), settings);
            xd.Load(reader);
#pragma warning restore RS0030 // Do not used banned APIs
            Single f = Single.Parse(xd.SelectSingleNode("//book/price").InnerText);
            return f;
        }

        public class LocalResolver : XmlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                // absoluteUri is the "identifier" immediately after SYSTEM or PUBLIC. If that "identifier" is not a URL, then absoluteUri contains a file:// with fully qualified path ending with the "uri" This is not what I´d expect, but anyway
                try
                {
                    var stream = GetResourceStream(absoluteUri.Segments[absoluteUri.Segments.Length - 1]);
                    if (stream != null) return stream;
                }
                catch  (Exception)
                { 
                    // throw error below
                }
                throw new UnexpectedExternalEntityEncounteredException();
            }

            static public Stream GetResourceStream(String localname)
            {
                // this is a quick and dirty solution. You might want to include the DTDs as a real resource into the executable instead. Or put them somewhere else
                // in any case you have to sanitize and preven path traversals. Never use the URL as passed in
                String a = Assembly.GetExecutingAssembly().Location;
                String s = Path.GetFullPath($@"{a}\..\..\..\..\..\Tests\Resources\{localname}");
                if (System.IO.File.Exists(s))
                    return new FileStream(s, FileMode.Open);
                return null;
            }

        }
    }
}
