using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BetriebsmittelPublisher.Models
{
    public class PgAutomationModel : INotifyPropertyChanged
    {
        private string _motorNumber;
        private List<PgTableRow> _tableRows;

        public string MotorNumber
        {
            get => _motorNumber;
            set
            {
                if (_motorNumber != value)
                {
                    _motorNumber = value;
                    OnPropertyChanged(nameof(MotorNumber));
                }
            }
        }

        public List<PgTableRow> TableRows
        {
            get => _tableRows;
            set
            {
                if (_tableRows != value)
                {
                    _tableRows = value;
                    OnPropertyChanged(nameof(TableRows));
                }
            }
        }

        public PgAutomationModel()
        {
            MotorNumber = string.Empty;
            TableRows = new List<PgTableRow>();
            InitializeTableRows();
        }

        private void InitializeTableRows()
        {
            TableRows.Clear();
            for (int i = 1; i <= 10; i++)
            {
                TableRows.Add(new PgTableRow(i));
            }
        }

        public void ClearTable()
        {
            InitializeTableRows();
            OnPropertyChanged(nameof(TableRows));
        }

        public bool ValidateMotorNumber(out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(MotorNumber))
            {
                error = "Motor number is required";
                return false;
            }

            if (!int.TryParse(MotorNumber, out int number))
            {
                error = "Motor number must be numeric";
                return false;
            }

            if (number < 1 || number > 999999)
            {
                error = "Motor number must be between 1 and 999999";
                return false;
            }

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}