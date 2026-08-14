using System;

namespace BetriebsmittelPublisher.Models
{
    public class PgTableRow
    {
        public int RowNumber { get; set; }
        public string MotorNumber { get; set; } = string.Empty;
        public string PgNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Error { get; set; } = string.Empty;

        public PgTableRow(int rowNumber)
        {
            RowNumber = rowNumber;
            Status = "pending";
            Timestamp = DateTime.MinValue;
        }
    }
}