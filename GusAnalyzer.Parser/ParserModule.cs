using Autofac;

namespace GusAnalyzer.Parser
{
    public class ParserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GusAnalyzer.Parser.Parser>().As<IParser>();
        }
    }
}
