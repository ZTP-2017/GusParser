using System;
using System.Collections.Generic;
using System.Text;
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
        private List<City> cities;

        public void Init()
        {
            var container = AutofacContainer.Configure();
        }

        public void ParseElements()
        {
            XDocument xdocTerc = XDocument.Load("TERC.xml");
            //XDocument xdocSimc = XDocument.Load("SIMC.xml");
            var simc = GetSimc(XDocument.Load("SIMC.xml"));
            XDocument xdocUlic = XDocument.Load("ULIC.xml");

            regions = GetRegions(xdocTerc);
            districts = GetDistricts(xdocTerc, regions);
            communes = GetCommunes(xdocTerc, districts);
            cities = GetCities(simc, communes);

            var streets = GetStreets(xdocUlic);
            var test = streets.Where(x => x.CityName == "Wieprz");
        }

        private Dictionary<string, string> GetSimc(XDocument xdocSimc)
        {

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

                if (!result.Any(y => y.DistrictId == districtId && y.RegionId == regionId))
                {
                    var region = regions.FirstOrDefault(z => z.RegionId == regionId);

                    result.Add(new District
                    {
                        DistrictId = districtId,
                        DistrictName = row.Element("NAZWA").Value,
                        RegionId = region.RegionId,
                        RegionName = region.RegionName
                    });
                }
            }

            return result;
        }

        private List<Commune> GetCommunes(XDocument terc, List<District> districts)
        {
            var result = new List<Commune>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                !string.IsNullOrEmpty(x.Element("POW").Value) && !string.IsNullOrEmpty(x.Element("GMI").Value)))
            {
                var regionId = row.Element("WOJ").Value;
                var districtId = row.Element("POW").Value;
                var communeId = row.Element("GMI").Value;

                if (!result.Any(y => y.CommuneId == communeId && y.RegionId == regionId && y.DistrictId == districtId))
                {
                    var district = districts.FirstOrDefault(z => z.DistrictId == districtId);

                    result.Add(new Commune
                    {
                        CommuneId = communeId,
                        CommuneName = row.Element("NAZWA").Value,
                        RegionId = district.RegionId,
                        RegionName = district.RegionName,
                        DistrictId = district.DistrictId,
                        DistrictName = district.DistrictName
                    });
                }
            }

            return result;
        }

        private List<City> GetCities(Dictionary<string, string> simc, List<Commune> communes)
        {
            var result = new List<City>();

            foreach (var row in simc.Descendants("row"))
            {
                var regionId = row.Element("WOJ").Value;
                var districtId = row.Element("POW").Value;
                var communeId = row.Element("GMI").Value;
                var name = row.Element("NAZWA").Value;
                var sym = row.Element("SYM").Value;

                result.Add(new City
                {
                    RegionId = regionId,
                    DistrictId = districtId,
                    CommuneId = communeId,
                    CityName = IsNumeric(sym) ? name : sym + " " + name
                });
            }

            return result;
        }

        private List<Street> GetStreets(XDocument ulic)
        {
            var result = new List<Street>(); var i = 1;

            foreach (var row in ulic.Descendants("row"))
            {
                var regionId = row.Element("WOJ").Value;
                var districtId = row.Element("POW").Value;
                var communeId = row.Element("GMI").Value;
                var attribute = row.Element("CECHA").Value;
                var name_1 = row.Element("NAZWA_1").Value;
                var name_2 = row.Element("NAZWA_2").Value;

                result.Add(new Street()
                {
                    RegionId = regionId,
                    RegionName = regions.FirstOrDefault(x => x.RegionId == regionId).RegionName,
                    DistrictId = districtId,
                    DistrictName = districts.FirstOrDefault(x => x.DistrictId == districtId).DistrictName,
                    CommuneId = communeId,
                    CommuneName = communes.FirstOrDefault(x => x.CommuneId == communeId).CommuneName,
                    StreetName = attribute + " " + (string.IsNullOrEmpty(name_2) ? name_1 : name_2 + " " + name_1),
                    CityName = cities.FirstOrDefault(x => x.RegionId == regionId && x.DistrictId == districtId).CityName
                });

                Console.WriteLine(i); i++;
            }

            return result;
        }

        private bool IsNumeric(string value)
        {
            double retNum;

            bool isNum = Double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
    }
}
