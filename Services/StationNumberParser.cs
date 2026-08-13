using System;
using System.Text.RegularExpressions;

namespace BetriebsmittelPublisher.Services
{
    public static class StationNumberParser
    {
        public static int? ExtractStationNumber(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return null;

            // Primary pattern: extract numeric value after /station/
            var stationMatch = Regex.Match(topic, @"/station/(\d+)");
            if (stationMatch.Success)
            {
                if (int.TryParse(stationMatch.Groups[1].Value, out int stationNumber))
                    return stationNumber;
            }

            // Fallback: extract first numeric segment in topic
            var fallbackMatch = Regex.Match(topic, @"\d+");
            if (fallbackMatch.Success)
            {
                if (int.TryParse(fallbackMatch.Value, out int fallbackNumber))
                    return fallbackNumber;
            }

            return null;
        }
    }
}