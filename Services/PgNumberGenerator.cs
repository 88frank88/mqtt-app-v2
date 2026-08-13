using System;
using System.Text;

namespace BetriebsmittelPublisher.Services
{
    public class PgNumberGeneratorConfig
    {
        public string Prefix { get; set; } = "PG";
        public int StartNumber { get; set; } = 1;
        public int Increment { get; set; } = 1;
        public int MaxRows { get; set; } = 10;
    }

    public class PgNumberGenerator
    {
        private readonly PgNumberGeneratorConfig _config;

        public PgNumberGenerator() : this(new PgNumberGeneratorConfig())
        {
        }

        public PgNumberGenerator(PgNumberGeneratorConfig config)
        {
            _config = config;
        }

        public string[] GeneratePgNumbers(int motorNumber, int rowCount)
        {
            if (rowCount > _config.MaxRows)
                rowCount = _config.MaxRows;

            var pgNumbers = new string[rowCount];
            var generatedNumbers = new System.Collections.Generic.HashSet<string>();
            var timestamp = DateTime.Now;

            for (int row = 0; row < rowCount; row++)
            {
                var pgNumber = GenerateUniquePgNumber(motorNumber, row + 1, timestamp, generatedNumbers);
                generatedNumbers.Add(pgNumber);
                pgNumbers[row] = pgNumber;
                
                // Increment timestamp by 1 second for each row to prevent collisions
                timestamp = timestamp.AddSeconds(1);
            }

            return pgNumbers;
        }

        private string GenerateUniquePgNumber(int motorNumber, int rowNumber, DateTime timestamp, System.Collections.Generic.HashSet<string> existingNumbers)
        {
            var timestampStr = timestamp.ToString("yyyyMMddHHmmss");
            
            while (true)
            {
                var pgNumber = $"{_config.Prefix}-{motorNumber}-{rowNumber}-{timestampStr}";
                
                if (!existingNumbers.Contains(pgNumber))
                {
                    return pgNumber;
                }
                
                // Add microsecond to handle collisions
                timestamp = timestamp.AddMilliseconds(timestamp.Millisecond);
            }
        }

        public bool ValidateMotorNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return int.TryParse(input, out int number) && number >= 1 && number <= 999999;
        }

        public string GetGenerationError()
        {
            return "Invalid motor number. Please enter a number between 1 and 999999.";
        }
    }
}