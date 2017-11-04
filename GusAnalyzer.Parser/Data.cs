using System.Xml.Linq;

namespace GusAnalyzer.Parser
{
    public class Data : IData
    {
        public XDocument LoadTerc()
        {
            return XDocument.Load("TERC.xml");
        }

        public XDocument LoadSimc()
        {
            return XDocument.Load("SIMC.xml");
        }

        public XDocument LoadUlic()
        {
            return XDocument.Load("ULIC.xml");
        }
    }
}
