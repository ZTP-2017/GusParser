using System.Collections.Generic;
using GusAnalyzer.Parser.Model;
using Autofac;
using System.Xml.Linq;
using System.Linq;

namespace GusAnalyzer.Parser
{
    public class Parser : IParser
    {
        private List<Region> regions;
        private List<District> districts;
        private List<Commune> communes;

        public void Init()
        {
            var container = AutofacContainer.Configure();
        }

        public List<GusItem> Parse()
        {
            XDocument xdocTerc = XDocument.Load("TERC.xml");
            regions = GetRegions(xdocTerc);
            districts = GetDistricts(xdocTerc, regions);
            communes = GetCommunes(xdocTerc, districts);

            var simc = GetSimc(XDocument.Load("SIMC.xml"));
            var ulic = GetUlic(XDocument.Load("ULIC.xml"));
            
            return GetResult(ulic, simc);
        }

        private List<Region> GetRegions(XDocument terc)
        {
            return terc.Descendants("row")
                .Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) && string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value))
                .Select(x => new Region(x.Element("WOJ").Value, x.Element("NAZWA").Value)).ToList();
        }

        private List<District> GetDistricts(XDocument terc, List<Region> regions)
        {
            var result = new List<District>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                !string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value)))
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

            return result;
        }

        private List<Commune> GetCommunes(XDocument terc, List<District> districts)
        {
            var result = new List<Commune>();
            
            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("RODZ").Value)))
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

            return result;
        }

        private Dictionary<string, List<City>> GetSimc(XDocument xdocSimc)
        {
            var result = new Dictionary<string, List<City>>();

            foreach (var row in xdocSimc.Descendants("row"))
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

            return result;
        }

        private Dictionary<string, List<Street>> GetUlic(XDocument xdocUlic)
        {
            var result = new Dictionary<string, List<Street>>();

            foreach (var row in xdocUlic.Descendants("row"))
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

            return result;
        }

        private List<GusItem> GetResult(Dictionary<string, List<Street>> ulic, Dictionary<string, List<City>> simc)
        {
            var result = new List<GusItem>();

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

            return result;
        }
    }
}
