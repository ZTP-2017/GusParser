using GusAnalyzer.Parser.Models;
using System.Collections.Generic;

namespace GusAnalyzer.Parser
{
    public interface IParser
    {
        List<Street> Parse();
    }
}
