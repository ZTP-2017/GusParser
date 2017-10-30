using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace GusAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new GusAnalyzer.Parser.Parser();
            parser.Init();
            parser.ParseElements();

            Console.ReadKey();
        }
    }
}
