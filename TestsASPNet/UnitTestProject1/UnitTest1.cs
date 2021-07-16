using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using Tests.Controllers;
using static Tests.Controllers.XmlValidatingInputController;

namespace UnitTestProject1
{
    [TestClass]
    public class DemoBadValidation
    {
        [TestMethod]
        [ExpectedException(typeof(XmlException))] // because &h; not understood without DTD
        public void GetWithCorrectDtdAndBadContent()
        {
            var c = new XmlNonValidatingInputController();
            Assert.AreEqual(1995, c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>"));
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))] // because &h; not understood without DTD
        public void GetNoDtdBadContent()
        {
            var c = new XmlNonValidatingInputController();
            Assert.AreEqual(1995, c.Get("<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>"));
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))] // no such host is known
        public void GetBadExternalResolver()
        {
            var c = new XmlValidatingWithExternalResolverController();
            Assert.AreEqual(1995, c.Get("<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>"));
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))] // forbidden
        public void GetBadExternalResolverWithDtd()
        {
            var c = new XmlValidatingWithExternalResolverController();
            Assert.AreEqual(1995, c.Get("<!DOCTYPE book SYSTEM 'http://localhost/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>"));
        }
    }

    [TestClass]
    public class DemoGoodValidation
    {

        // XML/DTD snippets based on https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument.xmlresolver?view=net-5.0
        // sure, XML is probably more likely as an http content than a URL arg, but I choose this for simplicity

        [TestMethod]
        public void Novalidation()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
        }
        [TestMethod]
        public void GetNoDTD()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
        }

        [TestMethod]
        public void GetWithCorrectDtdAndContent()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void GetWithUnknownDtdAndContent()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/unknown.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
        }
        [TestMethod]
        public void GetMissingDtdAndContent()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<?xml version=\"1.0\"?><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationFailedException))]
        public void GetWithCorrectDtdAndBadContent()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>"));
        }

        [TestMethod]
        public void GetWithSystemDtd()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE book SYSTEM \"Books.dtd\">\r\n<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/Note.dtd}
        }

        [TestMethod]
        public void GetWithPublicDtd()
        {
            var c = new XmlValidatingInputController();
            Assert.AreEqual(1995, c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE book PUBLIC \"Books.dtd\" \"Ingored.dtd\">\r\n<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>"));
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/identifier}
        }

        [TestMethod]
        public void GetXsdBooks()
        {
            var c = new SoapController();
            var s = new StreamReader(LocalResolver.GetResourceStream("bookstore.xml")).ReadToEnd();
            Assert.AreEqual(899, c.Get(s));
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/identifier}
        }
    }
}
