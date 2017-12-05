using System.Xml.Linq;
using GusAnalyzer.Parser.Interfaces;

namespace GusAnalyzer.Parser
{
    public class SimcData: ISimcData
    {
        private readonly XDocument _simc;

        public SimcData(XDocument simc)
        {
            _simc = simc;
        }

        public XDocument GetSimc()
        {
            return _simc;
        }
    }
}
