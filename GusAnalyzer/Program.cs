using System;

namespace GusAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new GusAnalyzer.Parser.Parser();
            var data = parser.Parse();

            Console.ReadKey();
        }
    }
}
