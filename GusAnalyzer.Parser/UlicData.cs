using System.Xml.Linq;
using GusAnalyzer.Parser.Interfaces;

namespace GusAnalyzer.Parser
{
    public class UlicData : IUlicData
    {
        private readonly XDocument _ulic;

        public UlicData(XDocument ulic)
        {
            _ulic = ulic;
        }

        public XDocument GetUlic()
        {
            return _ulic;
        }
    }
}
