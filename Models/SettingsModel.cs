namespace BetriebsmittelPublisher.Models
{
    public class SettingsModel
    {
        public string Betriebsmittel1Topic { get; set; } = "procon/bm1/data";
        public string Betriebsmittel2Topic { get; set; } = "procon/bm2/data";
        public string Betriebsmittel3Topic { get; set; } = "procon/bm3/data";
        public string Betriebsmittel4Topic { get; set; } = "procon/bm4/data";

        public int? StationNumber1 => Services.StationNumberParser.ExtractStationNumber(Betriebsmittel1Topic);
        public int? StationNumber2 => Services.StationNumberParser.ExtractStationNumber(Betriebsmittel2Topic);
        public int? StationNumber3 => Services.StationNumberParser.ExtractStationNumber(Betriebsmittel3Topic);
        public int? StationNumber4 => Services.StationNumberParser.ExtractStationNumber(Betriebsmittel4Topic);
    }
}