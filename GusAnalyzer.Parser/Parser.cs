using System.Collections.Generic;
using GusAnalyzer.Parser.Model;
using System.Xml.Linq;
using System.Linq;
using Serilog;
using System;

namespace GusAnalyzer.Parser
{
    public class Parser : IParser
    {
        private List<Region> regions;
        private List<District> districts;
        private List<Commune> communes;
        private IData data;

        public Parser()
        {
            if (data == null)
            {
                data = new Data();
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("logs/log-{Date}.txt")
                .CreateLogger();
        }

        public List<GusItem> Parse()
        {
            Log.Information("Start parsing");
            XDocument xdocTerc = data.LoadTerc();

            regions = GetRegions(xdocTerc);
            districts = GetDistricts(xdocTerc, regions);
            communes = GetCommunes(xdocTerc, districts);

            var simc = GetSimc(data.LoadSimc());
            var ulic = GetUlic(data.LoadUlic());
            
            return GetResult(ulic, simc);
        }

        private List<Region> GetRegions(XDocument terc)
        {
            var regions = new List<Region>();

            try
            {
                regions = terc.Descendants("row")
                    .Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) && string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value))
                    .Select(x => new Region(x.Element("WOJ").Value, x.Element("NAZWA").Value)).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Get regions error");
            }

            return regions;
        }

        private List<District> GetDistricts(XDocument terc, List<Region> regions)
        {
            var result = new List<District>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                !string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value)))
            {
                try
                {
                    var regionId = row.Element("WOJ").Value;
                    var districtId = row.Element("POW").Value;

                    var region = regions.FirstOrDefault(z => z.RegionId == regionId);

                    result.Add(new District
                    {
                        DistrictId = districtId,
                        DistrictName = row.Element("NAZWA").Value,
                        RegionId = region.RegionId,
                        RegionName = region.RegionName
                    });
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Get district error");
                }
            }

            return result;
        }

        private List<Commune> GetCommunes(XDocument terc, List<District> districts)
        {
            var result = new List<Commune>();
            
            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("RODZ").Value)))
            {
                try
                {
                    var regionId = row.Element("WOJ").Value;
                    var districtId = row.Element("POW").Value;
                    var communeId = row.Element("GMI").Value;
                    var tercId = regionId + "-" + districtId + "-" + communeId;

                    var district = districts.FirstOrDefault(z => z.RegionId == regionId && z.DistrictId == districtId);

                    result.Add(new Commune
                    {
                        CommuneId = communeId,
                        CommuneName = row.Element("NAZWA").Value,
                        RegionId = district.RegionId,
                        RegionName = district.RegionName,
                        DistrictId = district.DistrictId,
                        DistrictName = district.DistrictName,
                        TercId = tercId
                    });
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Get commune error");
                }
            }

            return result;
        }

        private Dictionary<string, List<City>> GetSimc(XDocument xdocSimc)
        {
            var result = new Dictionary<string, List<City>>();

            foreach (var row in xdocSimc.Descendants("row"))
            {
                try
                {
                    var key = row.Element("WOJ").Value + "-" + row.Element("POW").Value + "-" + row.Element("GMI").Value;
                    var simc = new City()
                    {
                        CityName = row.Element("NAZWA").Value,
                        CityId = row.Element("SYM").Value
                    };

                    if (result.ContainsKey(key))
                    {
                        result[key].Add(simc);
                    }
                    else
                    {
                        result.Add(key, new List<City>() { simc });
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Get simc error");
                }
            }

            return result;
        }

        private Dictionary<string, List<Street>> GetUlic(XDocument xdocUlic)
        {
            var result = new Dictionary<string, List<Street>>();

            foreach (var row in xdocUlic.Descendants("row"))
            {
                try
                {
                    var key = row.Element("WOJ").Value + "-" + row.Element("POW").Value + "-" + row.Element("GMI").Value;
                    var attribute = row.Element("CECHA").Value;
                    var name_1 = row.Element("NAZWA_1").Value;
                    var name_2 = row.Element("NAZWA_2").Value;
                    var name = attribute + " " + (string.IsNullOrEmpty(name_2) ? name_1 : name_2 + " " + name_1);

                    var ulic = new Street()
                    {
                        StreetName = name,
                        StreetId = row.Element("SYM").Value
                    };

                    if (result.ContainsKey(key))
                    {
                        result[key].Add(ulic);
                    }
                    else
                    {
                        result.Add(key, new List<Street>() { ulic });
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Get ulic error");
                }
            }

            return result;
        }

        private List<GusItem> GetResult(Dictionary<string, List<Street>> ulic, Dictionary<string, List<City>> simc)
        {
            var result = new List<GusItem>();
            Log.Information("Parsing data");

            foreach (var u in ulic)
            {
                var tmp = u.Key.Split("-");
                var regionId = tmp[0];
                var districtId = tmp[1];

                foreach (var street in u.Value)
                {
                    foreach (var city in simc[u.Key].Where(x => x.CityId == street.StreetId))
                    {
                        result.Add(new GusItem()
                        {
                            Region = regions.FirstOrDefault(x => x.RegionId == regionId).RegionName,
                            District = districts.FirstOrDefault(x => x.RegionId == regionId && x.DistrictId == districtId).DistrictName,
                            Commune = communes.FirstOrDefault(x => x.TercId == u.Key).CommuneName,
                            Street = street.StreetName,
                            City = city.CityName
                        });
                    }
                }
            }

            Log.Information("Parsing success. Parsed " + result.Count + " items");

            return result;
        }
    }
}
