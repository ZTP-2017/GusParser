using System;

namespace GusAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new GusAnalyzer.Parser.Parser();
            parser.Init();
            var data = parser.Parse();

            Console.ReadKey();
        }
    }
}
