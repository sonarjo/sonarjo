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
        //
        // use https://www.urlencoder.io/ to encode XML to URL encoded  (if necessary, often works without encoding)
        // <test><value>3</value></test> => %3Ctest%3E%3Cvalue%3E3%3C%2Fvalue%3E%3C%2Ftest%3E
        // => /xmlinput?arg=%3Ctest%3E%3Cvalue%3E3%3C%2Fvalue%3E%3C%2Ftest%3E
        [HttpGet]
        public Single Get(String arg)
        {
            // this is not the best input validation possible, we throw an exception early where probably we would continue to
            // validate more - or terminate with less overhead than an exception
            // but kiss for the sake of the example
            var settings = new XmlReaderSettings { ValidationType = ValidationType.DTD, DtdProcessing = DtdProcessing.Parse };
            settings.ValidationEventHandler += (s,e) => {
                BadRequest(); // BadRequest does not stop processing.
                throw new ValidationFailedException(e.Message, e.Exception);
            };
            settings.XmlResolver = new PreventExternalResolutionException();
            if (!arg.Contains("<!DOCTYPE"))
            {
                int insertpos = arg.StartsWith("<?xml") ? arg.IndexOf("?>") + 2 : 0;
                arg = arg.Substring(0, insertpos) + DefaultDtd + arg.Substring(insertpos);
            }
            XmlDocument xd = new XmlDocument();
            var reader = XmlReader.Create(new StringReader(arg), settings);
            xd.Load(reader);
            Single f = Single.Parse(xd.SelectSingleNode("//book/price").InnerText);
            return f;
        }

        private class PreventExternalResolutionException : XmlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                // absoluteUri is the "identifier" immediately after SYSTEM or PUBLIC. If that "identifier" is not a URL, then absoluteUri contains a file:// with fully qualified path ending with the "uri" This is not what I´d expect, but anyway
                try {
                    // this is a quick and dirty solution. You might want to include the DTDs as a real resource into the executable instead. Or put them somewhere else
                    // in any case you have to sanitize and preven path traversals. Never use the URL as passed in
                    String s = absoluteUri.Segments[absoluteUri.Segments.Length - 1];
                    String a = Assembly.GetExecutingAssembly().Location;
                    s = Path.GetFullPath($@"{a}\..\..\..\..\..\Tests\Resources\{s}");
                    if (System.IO.File.Exists(s))        
                        return new FileStream(s, FileMode.Open);
                    }
                catch  (Exception)
                { 
                    // throw error below
                }
                throw new UnexpectedExternalEntityEncounteredException();
            }
        }
    }
}
