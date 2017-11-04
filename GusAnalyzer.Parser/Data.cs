using Serilog;
using System;
using System.Xml.Linq;

namespace GusAnalyzer.Parser
{
    public class Data : IData
    {
        public XDocument LoadTerc()
        {
            XDocument terc = null;
            Log.Information("Loading terc");

            try
            {
                terc = XDocument.Load("TERC.xml");
                Log.Information("Loading terc success");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Loading terc error");
            }

            return terc;
        }

        public XDocument LoadSimc()
        {
            XDocument simc = null;
            Log.Information("Loading simc");

            try
            {
                simc = XDocument.Load("SIMC.xml");
                Log.Information("Loading simc success");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Loading simc error");
            }
            
            return simc;
        }

        public XDocument LoadUlic()
        {
            XDocument ulic = null;
            Log.Information("Loading ulic");

            try
            {
                ulic = XDocument.Load("ULIC.xml");
                Log.Information("Loading ulic success");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Loading ulic error");
            }

            return ulic;
        }
    }
}
