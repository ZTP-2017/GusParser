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
        public void Init()
        {
            var container = AutofacContainer.Configure();
        }

        public void ParseElements()
        {
            // var result = new List<ParsedObject>();

            XDocument xdocTerc = XDocument.Load("TERC.xml");
            XDocument xdocSimc = XDocument.Load("SIMC.xml");
            //XDocument xdocUlic = XDocument.Load("ULIC.xml");

            var regions = GetRegions(xdocTerc);
            var districts = GetDistricts(xdocTerc, regions);
            var communes = GetCommunes(xdocTerc, regions, districts);
            var cities = GetCities(xdocSimc, regions, districts, communes);


          //  return result;
        }

        public List<Region> GetRegions(XDocument terc)
        {
            var result = new List<Region>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value)))
            {
                result.Add(new Region
                {
                    RegionId = int.Parse(row.Element("WOJ").Value),
                    RegionName = row.Element("NAZWA").Value
                });
            }

            return result;
        }

        public List<District> GetDistricts(XDocument terc, List<Region> regions)
        {
            var result = new List<District>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                !string.IsNullOrEmpty(x.Element("POW").Value) && string.IsNullOrEmpty(x.Element("GMI").Value)))
            {
                var regionId = int.Parse(row.Element("WOJ").Value);
                var id = int.Parse(row.Element("POW").Value);

                if (!result.Any(y => y.DistrictId == id && y.RegionId == regionId))
                {
                    var region = regions.FirstOrDefault(z => z.RegionId == regionId);

                    result.Add(new District
                    {
                        DistrictId = id,
                        DistrictName = row.Element("NAZWA").Value,
                        RegionId = region.RegionId,
                        RegionName = region.RegionName
                    });
                }
            }

            return result;
        }

        public List<Commune> GetCommunes(XDocument terc, List<District> districts)
        {
            var result = new List<Commune>();

            foreach (var row in terc.Descendants("row").Where(x => !string.IsNullOrEmpty(x.Element("WOJ").Value) &&
                !string.IsNullOrEmpty(x.Element("POW").Value) && !string.IsNullOrEmpty(x.Element("GMI").Value)))
            {
                var regionId = int.Parse(row.Element("WOJ").Value);
                var districtId = int.Parse(row.Element("POW").Value);
                var id = int.Parse(row.Element("GMI").Value);

                if (!result.Any(y => y.CommuneId == id && y.RegionId == regionId && y.DistrictId == districtId))
                {
                    var district = districts.FirstOrDefault(z => z.DistrictId == districtId);
                    
                    result.Add(new Commune
                    {
                        CommuneId = id,
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

        public List<City> GetCities(XDocument simc, List<Commune> communes)
        {
            var result = new List<City>();
            foreach (var row in simc.Descendants("row"))
            {
                var regionId = int.Parse(row.Element("WOJ").Value);
                var districtId = int.Parse(row.Element("POW").Value);
                var communeId = int.Parse(row.Element("GMI").Value);
                var cityName = row.Element("NAZWA").Value;

                if (!result.Any(y => y.CommuneId == communeId && y.RegionId == regionId && y.DistrictId == districtId && y.CityName == cityName))
                {
                    var commune = communes.FirstOrDefault(z => z.CommuneId == communeId);

                    result.Add(new City
                    {
                        RegionId = commune.RegionId,
                        RegionName = commune.RegionName,
                        DistrictId = commune.DistrictId,
                        DistrictName = commune.DistrictName,
                        CommuneId = commune.CommuneId,
                        CommuneName = commune.CommuneName,
                        CityName = cityName
                    });
                }
            }

            return result;
        }
    }
}
