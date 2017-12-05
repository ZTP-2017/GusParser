using System;
using System.Xml.Linq;
using GusAnalyzer.Parser;

namespace GusAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var terc = XDocument.Load("TERC.xml");
            var simc = XDocument.Load("SIMC.xml");
            var ulic = XDocument.Load("ULIC.xml");

            var parser = new Parser.Parser(new TercData(terc), new SimcData(simc), new UlicData(ulic));

            var parsedData = parser.Parse();

            Console.ReadKey();
        }
    }
}
