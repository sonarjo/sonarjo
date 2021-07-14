using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Tests.Controllers;

namespace UnitTestProject1
{
    [TestClass]
    public class PreventExternalResolution
    {

        // XML/DTD snippets based on https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument.xmlresolver?view=net-5.0

        [TestMethod]
        public void GetNoDTD()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
        }

        [TestMethod]
        public void GetWithCorrectDtdAndContent()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
        }

        [TestMethod]
        [ExpectedException(typeof(XmlException))]
        public void GetWithUnknownDtdAndContent()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/unknown.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
        }
        [TestMethod]
        public void GetMissingDtdAndContent()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<?xml version=\"1.0\"?><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationFailedException))]
        public void GetWithCorrectDtdAndBadContent()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<!DOCTYPE book SYSTEM 'http://myServer/data/books.dtd'><book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misdc>&h;</misdc></book>");
        }

        [TestMethod]
        public void GetWithSystemDtd()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE book SYSTEM \"Books.dtd\">\r\n<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/Note.dtd}
        }

        [TestMethod]
        public void GetWithPublicDtd()
        {
            XmlValidatingInputController c = new XmlValidatingInputController();
            c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE book PUBLIC \"Books.dtd\" \"Ingored.dtd\">\r\n<book ISBN = '1-861001-57-5'> <title>Oberon's Legacy</title><price>19.95</price><misc>&h;</misc></book>");
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/identifier}
        }
    }
}
