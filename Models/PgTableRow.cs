using System;

namespace BetriebsmittelPublisher.Models
{
    public class PgTableRow
    {
        public int RowNumber { get; set; }
        public string MotorNumber { get; set; }
        public string PgNumber { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string Error { get; set; }

        public PgTableRow(int rowNumber)
        {
            RowNumber = rowNumber;
            Status = "pending";
            Timestamp = DateTime.MinValue;
        }
    }
}