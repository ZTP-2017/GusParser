using GusAnalyzer.Parser.Model;
using System.Collections.Generic;

namespace GusAnalyzer.Parser
{
    public interface IParser
    {
        List<GusItem> Parse();
    }
}
