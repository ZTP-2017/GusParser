using System.Xml.Linq;

namespace GusAnalyzer.Parser
{
    public interface IData
    {
        XDocument LoadTerc();
        XDocument LoadSimc();
        XDocument LoadUlic();
    }
}
