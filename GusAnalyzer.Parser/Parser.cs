using System.Collections.Generic;
using System.Linq;
using GusAnalyzer.Parser.Interfaces;
using Serilog;
using GusAnalyzer.Parser.Models;

namespace GusAnalyzer.Parser
{
    public class Parser : IParser
    {
        private readonly ISimcData _simcData;
        private readonly ITercData _tercData;
        private readonly IUlicData _ulicData;

        public Parser(ITercData tercData, ISimcData simcData, IUlicData ulicData)
        {
            _simcData = simcData;
            _tercData = tercData;
            _ulicData = ulicData;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("logs/log-{Date}.txt")
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }

        public List<Street> Parse()
        {
            Log.Information("Loading Regions");
            var regions = _tercData.GetTerc().Descendants().Where(x =>
                string.IsNullOrEmpty(x.Element("GMI")?.Value) &&
                string.IsNullOrEmpty(x.Element("POW")?.Value) &&
                !string.IsNullOrEmpty(x.Element("WOJ")?.Value)).Select(x => new
            {
                Id = x.Element("WOJ")?.Value,
                RegionName = x.Element("NAZWA")?.Value
            }).ToList();

            Log.Information("Loading Districts");
            var districts = _tercData.GetTerc().Descendants().Where(x =>
                string.IsNullOrEmpty(x.Element("GMI")?.Value) &&
                !string.IsNullOrEmpty(x.Element("POW")?.Value) &&
                !string.IsNullOrEmpty(x.Element("WOJ")?.Value)).Select(x => new
            {
                Id = $"{x.Element("WOJ")?.Value}-{x.Element("POW")?.Value}",
                DistrictName = x.Element("NAZWA")?.Value
            }).ToList();

            Log.Information("Loading Communes");
            var communes = _tercData.GetTerc().Descendants().Where(x =>
                !string.IsNullOrEmpty(x.Element("GMI")?.Value) &&
                !string.IsNullOrEmpty(x.Element("POW")?.Value) &&
                !string.IsNullOrEmpty(x.Element("WOJ")?.Value)).Select(x => new
            {
                Id = $"{x.Element("WOJ")?.Value}-{x.Element("POW")?.Value}-{x.Element("GMI")?.Value}-{x.Element("RODZ")?.Value}",
                CommuneName = x.Element("NAZWA")?.Value,
                CommuneType = x.Element("NAZWA_DOD")?.Value
            }).ToList();

            Log.Information("Start Parsing");
            var result = _ulicData.GetUlic().Descendants()
                .Where(x =>
                    x.Element("CECHA") != null &&
                    x.Element("NAZWA_1") != null &&
                    x.Element("NAZWA_2") != null)
                .Select(x => new
                {
                    StreetName = x.Element("CECHA")?.Value + " " + (string.IsNullOrEmpty(x.Element("NAZWA_2")?.Value)
                        ? x.Element("NAZWA_1")?.Value
                        : x.Element("NAZWA_2")?.Value + " " + x.Element("NAZWA_1")?.Value),
                    StreetId = x.Element("SYM")?.Value
                }).Join(_simcData.GetSimc().Descendants(), x => x.StreetId, y => y.Element("SYM")?.Value, (x, y) => new
                {
                    x.StreetName,
                    CityName = y.Element("NAZWA")?.Value,
                    RegionId = y.Element("WOJ")?.Value,
                    DistrictId = y.Element("POW")?.Value,
                    CommuneId = y.Element("GMI")?.Value,
                    CommuneTypeId = y.Element("RODZ_GMI")?.Value,
                    CommuneType = y.Element("NAZWA_DOD")?.Value
                }).Join(regions, x => x.RegionId, y => y.Id, (x, y) => new
                {
                    Id = $"{x.RegionId}-{x.DistrictId}",
                    x.CommuneId,
                    x.CommuneTypeId,
                    x.StreetName,
                    x.CityName,
                    x.CommuneType,
                    y.RegionName
                }).Join(districts, x => x.Id, y => y.Id, (x, y) => new
                {
                    Id = $"{x.Id}-{x.CommuneId}-{x.CommuneTypeId}",
                    x.StreetName,
                    x.CityName,
                    x.RegionName,
                    x.CommuneType,
                    y.DistrictName
                }).Join(communes, x => x.Id, y => y.Id, (x, y) => new Street()
                {
                    StreetName = x.StreetName,
                    CityName = x.CityName,
                    RegionName = x.RegionName,
                    DistrictName = x.DistrictName,
                    CommuneType = y.CommuneType,
                    CommuneName = y.CommuneName
                }).ToList();

            Log.Information("Finish Parsing");
            Log.Information("Parsing success. Parsed " + result.Count + " items");

            return result;
        }
    }
}
