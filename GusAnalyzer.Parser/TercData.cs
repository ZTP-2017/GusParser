using System.Xml.Linq;
using GusAnalyzer.Parser.Interfaces;

namespace GusAnalyzer.Parser
{
    public class TercData : ITercData
    {
        private readonly XDocument _terc;

        public TercData(XDocument terc)
        {
            _terc = terc;
        }

        public XDocument GetTerc()
        {
            return _terc;
        }
    }
}
