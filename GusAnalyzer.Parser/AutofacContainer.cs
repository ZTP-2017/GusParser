using Autofac;

namespace GusAnalyzer.Parser
{
    public class AutofacContainer
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<Parser>().As<IParser>();
            builder.RegisterModule<ParserModule>();

            return builder.Build();
        }
    }
}
