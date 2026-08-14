namespace BetriebsmittelPublisher.Models
{
    public class SettingsModel
    {
        public string BrokerAddress { get; set; } = "192.168.1.100";
        public int BrokerPort { get; set; } = 1883;
        public string ClientId { get; set; } = "BetriebsmittelPublisher";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public string Betriebsmittel1Topic { get; set; } = "procon/bm1/data";
        public string Betriebsmittel2Topic { get; set; } = "procon/bm2/data";
        public string Betriebsmittel3Topic { get; set; } = "procon/bm3/data";
        public string Betriebsmittel4Topic { get; set; } = "procon/bm4/data";

        public string MotorNumber { get; set; } = "";
        public string StationNumber { get; set; } = "";
        public string Quitk { get; set; } = "R";
        public string Tv { get; set; } = "37191";
        public string Ma { get; set; } = "0004808061";
        public string Bauart { get; set; } = "2013";
        public string ToolPosition { get; set; } = "1";
        public string ConnectTimeout { get; set; } = "10000";
        public string Dmc { get; set; } = "";
    }
}