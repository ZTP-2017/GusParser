namespace GusAnalyzer.Parser.Model
{
    public class Region
    {
        public string RegionId { get; set; }
        public string RegionName { get; set; }

        public Region(string regionId, string regionName)
        {
            RegionId = regionId;
            RegionName = regionName;
        }

        public Region()
        {

        }
    }
}
