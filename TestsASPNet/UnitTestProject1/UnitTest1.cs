using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Controllers;

namespace UnitTestProject1
{
    [TestClass]
    public class PreventExternalResolution
    {
        [TestMethod]
        public void GetNoDTD()
        {
            XmlInputControllerPreventExternalResolution c = new XmlInputControllerPreventExternalResolution();
            c.Get("<test><value>3</value></test>");
        }

        [TestMethod]
        [ExpectedException(typeof(ExternalEntityEncountered))]
        public void GetWithSystemDtd()
        {
            XmlInputControllerPreventExternalResolution c = new XmlInputControllerPreventExternalResolution();
            c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE note SYSTEM \"Note.dtd\">\r\n<test><value>3</value></test>");
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/Note.dtd}
        }

        [TestMethod]
        [ExpectedException(typeof(ExternalEntityEncountered))]
        public void GetWithPublicDtd()
        {
            XmlInputControllerPreventExternalResolution c = new XmlInputControllerPreventExternalResolution();
            c.Get("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE note PUBLIC \"identifier\" \"Note.dtd\">\r\n<test><value>3</value></test>");
            // {file:///C:/Users/LindenJo/source/repos/sonarjo/TestsASPNet/UnitTestProject1/bin/Debug/netcoreapp3.1/identifier}
        }
    }
}
