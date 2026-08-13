using System;
using System.Drawing;
using System.Windows.Forms;
using BetriebsmittelPublisher.Models;
using BetriebsmittelPublisher.Services;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.UI
{
    public class SettingsWindow : BaseForm
    {
        private TextBox _topic1TextBox;
        private TextBox _topic2TextBox;
        private TextBox _topic3TextBox;
        private TextBox _topic4TextBox;
        private Label _station1Label;
        private Label _station2Label;
        private Label _station3Label;
        private Label _station4Label;
        private Button _saveButton;
        private Button _cancelButton;
        private Label _validationLabel;

        private readonly SettingsModel _settings;

        public SettingsWindow()
        {
            _settings = SettingsPersistence.Load();
            InitializeComponent();
            LoadSettingsToForm();
        }

        private void InitializeComponent()
        {
            this.Text = $"Settings - {Core.VersionInfo.ShortInfo}";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 7,
                ColumnCount = 2
            };

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var headingLabel = new Label
            {
                Text = "MQTT Topic Configuration",
                Font = DesignSystem.Typography.GetSansFont(12, FontStyle.Bold),
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(headingLabel, 0, 0);
            mainPanel.SetColumnSpan(headingLabel, 2);

            CreateTopicInputGroup(mainPanel, 1, "Betriebsmittel 1:", ref _topic1TextBox, ref _station1Label);
            CreateTopicInputGroup(mainPanel, 2, "Betriebsmittel 2:", ref _topic2TextBox, ref _station2Label);
            CreateTopicInputGroup(mainPanel, 3, "Betriebsmittel 3:", ref _topic3TextBox, ref _station3Label);
            CreateTopicInputGroup(mainPanel, 4, "Betriebsmittel 4:", ref _topic4TextBox, ref _station4Label);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 40
            };

            _saveButton = new Button
            {
                Text = "Save",
                Size = new Size(100, 30),
                BackColor = DesignSystem.Colors.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;
            buttonPanel.Controls.Add(_saveButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(buttonPanel, 0, 5);
            mainPanel.SetColumnSpan(buttonPanel, 2);

            _validationLabel = new Label
            {
                Text = "",
                ForeColor = DesignSystem.Colors.Error,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(_validationLabel, 0, 6);
            mainPanel.SetColumnSpan(_validationLabel, 2);

            this.Controls.Add(mainPanel);

            _topic1TextBox.TextChanged += ValidateInput;
            _topic2TextBox.TextChanged += ValidateInput;
            _topic3TextBox.TextChanged += ValidateInput;
            _topic4TextBox.TextChanged += ValidateInput;
        }

        private void CreateTopicInputGroup(TableLayoutPanel parent, int rowIndex, string labelText, 
            ref TextBox textBox, ref Label stationLabel)
        {
            var label = new Label
            {
                Text = labelText,
                Font = DesignSystem.Typography.GetSansFont(9.5f),
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(label, 0, rowIndex);

            var groupPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Height = 60
            };

            textBox = new TextBox
            {
                Width = 400,
                Height = 25,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = DesignSystem.Typography.GetMonoFont(9)
            };
            groupPanel.Controls.Add(textBox);

            stationLabel = new Label
            {
                Text = "Station: -",
                Font = DesignSystem.Typography.GetSansFont(8),
                ForeColor = DesignSystem.Colors.TextSecondary,
                Height = 20
            };
            groupPanel.Controls.Add(stationLabel);

            parent.Controls.Add(groupPanel, 1, rowIndex);

            textBox.TextChanged += (s, e) => UpdateStationNumber(textBox, stationLabel);
        }

        private void UpdateStationNumber(TextBox textBox, Label stationLabel)
        {
            var stationNumber = StationNumberParser.ExtractStationNumber(textBox.Text);
            stationLabel.Text = stationNumber.HasValue ? $"Station: {stationNumber.Value}" : "Station: -";
        }

        private void ValidateInput(object sender, EventArgs e)
        {
            var isValid = ValidateTopic(_topic1TextBox.Text) &&
                          ValidateTopic(_topic2TextBox.Text) &&
                          ValidateTopic(_topic3TextBox.Text) &&
                          ValidateTopic(_topic4TextBox.Text);

            _saveButton.Enabled = isValid;
            _validationLabel.Text = isValid ? "" : "Invalid topic format. Use alphanumeric, /, _, - only. No spaces.";
        }

        private bool ValidateTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return false;

            if (topic.Contains(" "))
                return false;

            return Regex.IsMatch(topic, @"^[a-zA-Z0-9_\-/]+$");
        }

        private void LoadSettingsToForm()
        {
            _topic1TextBox.Text = _settings.Betriebsmittel1Topic;
            _topic2TextBox.Text = _settings.Betriebsmittel2Topic;
            _topic3TextBox.Text = _settings.Betriebsmittel3Topic;
            _topic4TextBox.Text = _settings.Betriebsmittel4Topic;

            UpdateStationNumber(_topic1TextBox, _station1Label);
            UpdateStationNumber(_topic2TextBox, _station2Label);
            UpdateStationNumber(_topic3TextBox, _station3Label);
            UpdateStationNumber(_topic4TextBox, _station4Label);

            ValidateInput(null, EventArgs.Empty);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateTopic(_topic1TextBox.Text) ||
                !ValidateTopic(_topic2TextBox.Text) ||
                !ValidateTopic(_topic3TextBox.Text) ||
                !ValidateTopic(_topic4TextBox.Text))
            {
                _validationLabel.Text = "Cannot save: Invalid topic format detected.";
                return;
            }

            _settings.Betriebsmittel1Topic = _topic1TextBox.Text;
            _settings.Betriebsmittel2Topic = _topic2TextBox.Text;
            _settings.Betriebsmittel3Topic = _topic3TextBox.Text;
            _settings.Betriebsmittel4Topic = _topic4TextBox.Text;

            if (SettingsPersistence.Save(_settings))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                _validationLabel.Text = "Failed to save settings. Please check file permissions.";
            }
        }
    }
}