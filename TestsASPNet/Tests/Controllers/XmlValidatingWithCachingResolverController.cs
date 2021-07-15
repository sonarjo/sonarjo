using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
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
    [Route("[validatingcachingcontroller]")]
    public class XmlValidatingWithCachingResolverController : ControllerBase
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
            var settings = new XmlReaderSettings { ValidationType = ValidationType.DTD, DtdProcessing = DtdProcessing.Parse, XmlResolver = new TheResolver(true) };
            settings.ValidationEventHandler += (s, e) => {
                BadRequest(); // BadRequest does not stop processing.
                throw new ValidationFailedException(e.Message, e.Exception);
            };
            XmlDocument xd = new XmlDocument();
            var reader = XmlReader.Create(new StringReader(arg), settings);
            xd.Load(reader);
            Single f = Single.Parse(xd.SelectSingleNode("//book/price").InnerText);
            return f;
        }

        // taken https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlurlresolver?view=net-5.0
        // renamed to challenge Sonar, faster but still unsafe
        class TheResolver : XmlUrlResolver
        {
            readonly bool enableHttpCaching;
            ICredentials credentials;

            //resolve resources from cache (if possible) when enableHttpCaching is set to true
            //resolve resources from source when enableHttpcaching is set to false
            public TheResolver(bool enableHttpCaching)
            {
                this.enableHttpCaching = enableHttpCaching;
            }

            public override ICredentials Credentials
            {
                set
                {
                    credentials = value;
                    base.Credentials = value;
                }
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                if (absoluteUri == null)
                {
                    throw new ArgumentNullException("absoluteUri");
                }
                //resolve resources from cache (if possible)
                if (absoluteUri.Scheme == "http" && enableHttpCaching && (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream)))
                {
                    WebRequest webReq = WebRequest.Create(absoluteUri);
                    webReq.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Default);
                    if (credentials != null)
                    {
                        webReq.Credentials = credentials;
                    }
                    WebResponse resp = webReq.GetResponse();
                    return resp.GetResponseStream();
                }
                //otherwise use the default behavior of the XmlUrlResolver class (resolve resources from source)
                else
                {
                    return base.GetEntity(absoluteUri, role, ofObjectToReturn);
                }
            }
        }
    }
}
