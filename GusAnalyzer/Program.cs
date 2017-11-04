using System;

namespace GusAnalyzer
{
    class Program
    {
        public Program()
        {

        }

        static void Main(string[] args)
        {
            var parser = new GusAnalyzer.Parser.Parser();
            var data = parser.Parse();

            Console.ReadKey();
        }
    }
}
