using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using BetriebsmittelPublisher.Models;
using BetriebsmittelPublisher.Services;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.UI
{
    public class SettingsWindow : BaseForm
    {
        private TextBox _brokerAddressTextBox = null!;
        private NumericUpDown _brokerPortNumeric = null!;
        private TextBox _clientIdTextBox = null!;
        private TextBox _usernameTextBox = null!;
        private TextBox _passwordTextBox = null!;
        private Button _testConnectionButton = null!;
        private Label _connectionTestLabel = null!;

        private TextBox _motorNumberTextBox = null!;
        private TextBox _quitkTextBox = null!;
        private TextBox _tvTextBox = null!;
        private TextBox _maTextBox = null!;
        private TextBox _bauartTextBox = null!;
        private TextBox _toolPositionTextBox = null!;
        private TextBox _connectTimeoutTextBox = null!;
        private TextBox _dmcTextBox = null!;

        private TextBox _topic1TextBox = null!;
        private TextBox _topic2TextBox = null!;
        private TextBox _topic3TextBox = null!;
        private TextBox _topic4TextBox = null!;
        private Label _station1Label = null!;
        private Label _station2Label = null!;
        private Label _station3Label = null!;
        private Label _station4Label = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Label _validationLabel = null!;

        private readonly SettingsModel _settings;

        public SettingsWindow()
        {
            _settings = SettingsPersistence.Load();
            InitializeComponent();
            LoadSettingsToForm();
        }

        private void InitializeComponent()
        {
            this.Text = $"Einstellungen - {Core.VersionInfo.ShortInfo}";
            this.Size = new Size(680, 1000);
            this.StartPosition = FormStartPosition.CenterParent;

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20),
                ColumnCount = 2
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // === Broker Section ===
            var brokerHeading = CreateHeading("MQTT Broker");
            mainPanel.Controls.Add(brokerHeading, 0, mainPanel.RowCount);
            mainPanel.SetColumnSpan(brokerHeading, 2);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Broker-Adresse:"), 0, mainPanel.RowCount);
            _brokerAddressTextBox = CreateTextBox(300);
            mainPanel.Controls.Add(_brokerAddressTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Port:"), 0, mainPanel.RowCount);
            _brokerPortNumeric = new NumericUpDown
            {
                Width = 100,
                Minimum = 1,
                Maximum = 65535,
                Value = 1883,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = DesignSystem.Typography.GetMonoFont(9)
            };
            mainPanel.Controls.Add(_brokerPortNumeric, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Client-ID:"), 0, mainPanel.RowCount);
            _clientIdTextBox = CreateTextBox(300);
            mainPanel.Controls.Add(_clientIdTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Benutzername:"), 0, mainPanel.RowCount);
            _usernameTextBox = CreateTextBox(300);
            mainPanel.Controls.Add(_usernameTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Passwort:"), 0, mainPanel.RowCount);
            _passwordTextBox = CreateTextBox(300);
            _passwordTextBox.UseSystemPasswordChar = true;
            mainPanel.Controls.Add(_passwordTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            // Test connection button + result label
            mainPanel.Controls.Add(CreateFieldLabel(""), 0, mainPanel.RowCount);
            var testPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 35, WrapContents = false };
            _testConnectionButton = new Button
            {
                Text = "Verbindung testen",
                Size = new Size(150, 30),
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _testConnectionButton.Click += TestConnectionButton_Click;
            testPanel.Controls.Add(_testConnectionButton);

            _connectionTestLabel = new Label
            {
                Text = "",
                AutoSize = true,
                ForeColor = DesignSystem.Colors.TextSecondary,
                Padding = new Padding(10, 5, 0, 0)
            };
            testPanel.Controls.Add(_connectionTestLabel);
            mainPanel.Controls.Add(testPanel, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            // === Topics Section ===
            var topicHeading = CreateHeading("MQTT Topics (Betriebsmittel)");
            mainPanel.Controls.Add(topicHeading, 0, mainPanel.RowCount);
            mainPanel.SetColumnSpan(topicHeading, 2);
            mainPanel.RowCount++;

            CreateTopicInputGroup(mainPanel, mainPanel.RowCount, "Betriebsmittel 1:", out _topic1TextBox, out _station1Label);
            mainPanel.RowCount++;
            CreateTopicInputGroup(mainPanel, mainPanel.RowCount, "Betriebsmittel 2:", out _topic2TextBox, out _station2Label);
            mainPanel.RowCount++;
            CreateTopicInputGroup(mainPanel, mainPanel.RowCount, "Betriebsmittel 3:", out _topic3TextBox, out _station3Label);
            mainPanel.RowCount++;
            CreateTopicInputGroup(mainPanel, mainPanel.RowCount, "Betriebsmittel 4:", out _topic4TextBox, out _station4Label);
            mainPanel.RowCount++;

            // === Automation Section ===
            var automationHeading = CreateHeading("Automation Parameter");
            mainPanel.Controls.Add(automationHeading, 0, mainPanel.RowCount);
            mainPanel.SetColumnSpan(automationHeading, 2);
            mainPanel.RowCount++;

            var motorLabel = CreateFieldLabel("Motor-Nummer (8-stellig):");
            mainPanel.Controls.Add(motorLabel, 0, mainPanel.RowCount);
            _motorNumberTextBox = CreateTextBox(200);
            _motorNumberTextBox.MaxLength = 8;
            mainPanel.Controls.Add(_motorNumberTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("QUITK:"), 0, mainPanel.RowCount);
            _quitkTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_quitkTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("TV:"), 0, mainPanel.RowCount);
            _tvTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_tvTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("MA:"), 0, mainPanel.RowCount);
            _maTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_maTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Bauart:"), 0, mainPanel.RowCount);
            _bauartTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_bauartTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Tool-Position:"), 0, mainPanel.RowCount);
            _toolPositionTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_toolPositionTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("Connect-Timeout (ms):"), 0, mainPanel.RowCount);
            _connectTimeoutTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_connectTimeoutTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            mainPanel.Controls.Add(CreateFieldLabel("DMC:"), 0, mainPanel.RowCount);
            _dmcTextBox = CreateTextBox(200);
            mainPanel.Controls.Add(_dmcTextBox, 1, mainPanel.RowCount);
            mainPanel.RowCount++;

            // === Buttons ===
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 45
            };

            _saveButton = new Button
            {
                Text = "Speichern",
                Size = new Size(110, 32),
                BackColor = DesignSystem.Colors.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;
            buttonPanel.Controls.Add(_saveButton);

            _cancelButton = new Button
            {
                Text = "Abbrechen",
                Size = new Size(110, 32),
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(CreateFieldLabel(""), 0, mainPanel.RowCount);
            mainPanel.Controls.Add(buttonPanel, 1, mainPanel.RowCount);
            mainPanel.SetColumnSpan(buttonPanel, 2);
            mainPanel.RowCount++;

            _validationLabel = new Label
            {
                Text = "",
                ForeColor = DesignSystem.Colors.Error,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(_validationLabel, 0, mainPanel.RowCount);
            mainPanel.SetColumnSpan(_validationLabel, 2);

            scrollPanel.Controls.Add(mainPanel);
            this.Controls.Add(scrollPanel);

            _topic1TextBox.TextChanged += ValidateInput;
            _topic2TextBox.TextChanged += ValidateInput;
            _topic3TextBox.TextChanged += ValidateInput;
            _topic4TextBox.TextChanged += ValidateInput;
            _brokerAddressTextBox.TextChanged += ValidateInput;
        }

        private Label CreateHeading(string text)
        {
            return new Label
            {
                Text = text,
                Font = DesignSystem.Typography.GetSansFont(12, FontStyle.Bold),
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 35,
                Margin = new Padding(0, 15, 0, 5)
            };
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = DesignSystem.Typography.GetSansFont(9.5f),
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 30
            };
        }

        private TextBox CreateTextBox(int width)
        {
            return new TextBox
            {
                Width = width,
                Height = 25,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = DesignSystem.Typography.GetMonoFont(9)
            };
        }

        private void CreateTopicInputGroup(TableLayoutPanel parent, int rowIndex, string labelText,
            out TextBox textBox, out Label stationLabel)
        {
            var label = CreateFieldLabel(labelText);
            parent.Controls.Add(label, 0, rowIndex);

            var groupPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Height = 60
            };

            textBox = CreateTextBox(420);
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

            var localTextBox = textBox;
            var localLabel = stationLabel;
            localTextBox.TextChanged += (s, e) => UpdateStationNumber(localTextBox, localLabel);
        }

        private void UpdateStationNumber(TextBox textBox, Label stationLabel)
        {
            var stationNumber = StationNumberParser.ExtractStationNumber(textBox.Text);
            stationLabel.Text = stationNumber.HasValue ? $"Station: {stationNumber.Value}" : "Station: -";
        }

        private async void TestConnectionButton_Click(object? sender, EventArgs e)
        {
            _testConnectionButton.Enabled = false;
            _connectionTestLabel.Text = "Verbinde...";
            _connectionTestLabel.ForeColor = DesignSystem.Colors.TextSecondary;
            Logger.Info($"Verbindungstest zu {_brokerAddressTextBox.Text}:{(int)_brokerPortNumeric.Value}");

            var testManager = new ConnectionManager();
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                await testManager.ConnectAsync(_brokerAddressTextBox.Text.Trim(), (int)_brokerPortNumeric.Value, cts.Token);

                var connectMessage = new MqttConnectMessage
                {
                    ClientId = string.IsNullOrWhiteSpace(_clientIdTextBox.Text) ? "BetriebsmittelPublisher-Test" : _clientIdTextBox.Text.Trim(),
                    Username = _usernameTextBox.Text,
                    Password = _passwordTextBox.Text
                };

                var connectPacket = MqttPacketBuilder.BuildConnectPacket(connectMessage);
                await testManager.SendPacketAsync(connectPacket, cts.Token);
                var response = await testManager.ReceivePacketAsync(cts.Token);

                bool accepted = response.Length >= 4 && response[3] == 0x00;
                if (accepted)
                {
                    _connectionTestLabel.Text = "Erfolgreich verbunden";
                    _connectionTestLabel.ForeColor = Color.FromArgb(76, 175, 80);
                    Logger.Info("Verbindungstest erfolgreich");
                }
                else
                {
                    byte returnCode = response.Length >= 4 ? response[3] : (byte)0xFF;
                    _connectionTestLabel.Text = $"Broker lehnte ab (Code {returnCode})";
                    _connectionTestLabel.ForeColor = DesignSystem.Colors.Warning;
                    Logger.Warning($"Verbindungstest: Broker lehnte ab (Code {returnCode})");
                }
            }
            catch (Exception ex)
            {
                _connectionTestLabel.Text = $"Fehlgeschlagen: {ex.Message}";
                _connectionTestLabel.ForeColor = DesignSystem.Colors.Error;
                Logger.Error("Verbindungstest fehlgeschlagen", ex);
            }
            finally
            {
                testManager.Dispose();
                _testConnectionButton.Enabled = true;
            }
        }

        private void ValidateInput(object? sender, EventArgs e)
        {
            var isValid = ValidateBrokerSettings() &&
                          ValidateTopic(_topic1TextBox.Text) &&
                          ValidateTopic(_topic2TextBox.Text) &&
                          ValidateTopic(_topic3TextBox.Text) &&
                          ValidateTopic(_topic4TextBox.Text);

            _saveButton.Enabled = isValid;
            if (!ValidateBrokerSettings())
                _validationLabel.Text = "Ungültige Broker-Einstellungen (Adresse erforderlich).";
            else
                _validationLabel.Text = isValid ? "" : "Ungültiges Topic-Format. Nur alphanumerisch, /, _, - erlaubt. Keine Leerzeichen.";
        }

        private bool ValidateBrokerSettings()
        {
            return !string.IsNullOrWhiteSpace(_brokerAddressTextBox.Text);
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
            _brokerAddressTextBox.Text = _settings.BrokerAddress;
            _brokerPortNumeric.Value = _settings.BrokerPort;
            _clientIdTextBox.Text = _settings.ClientId;
            _usernameTextBox.Text = _settings.Username;
            _passwordTextBox.Text = _settings.Password;

            _topic1TextBox.Text = _settings.Betriebsmittel1Topic;
            _topic2TextBox.Text = _settings.Betriebsmittel2Topic;
            _topic3TextBox.Text = _settings.Betriebsmittel3Topic;
            _topic4TextBox.Text = _settings.Betriebsmittel4Topic;

            _motorNumberTextBox.Text = _settings.MotorNumber;
            _quitkTextBox.Text = _settings.Quitk;
            _tvTextBox.Text = _settings.Tv;
            _maTextBox.Text = _settings.Ma;
            _bauartTextBox.Text = _settings.Bauart;
            _toolPositionTextBox.Text = _settings.ToolPosition;
            _connectTimeoutTextBox.Text = _settings.ConnectTimeout;
            _dmcTextBox.Text = _settings.Dmc;

            UpdateStationNumber(_topic1TextBox, _station1Label);
            UpdateStationNumber(_topic2TextBox, _station2Label);
            UpdateStationNumber(_topic3TextBox, _station3Label);
            UpdateStationNumber(_topic4TextBox, _station4Label);

            ValidateInput(null, EventArgs.Empty);
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateBrokerSettings())
            {
                _validationLabel.Text = "Cannot save: Broker-Adresse fehlt.";
                return;
            }

            if (!ValidateTopic(_topic1TextBox.Text) ||
                !ValidateTopic(_topic2TextBox.Text) ||
                !ValidateTopic(_topic3TextBox.Text) ||
                !ValidateTopic(_topic4TextBox.Text))
            {
                _validationLabel.Text = "Cannot save: Invalid topic format detected.";
                return;
            }

            _settings.BrokerAddress = _brokerAddressTextBox.Text.Trim();
            _settings.BrokerPort = (int)_brokerPortNumeric.Value;
            _settings.ClientId = _clientIdTextBox.Text.Trim();
            _settings.Username = _usernameTextBox.Text;
            _settings.Password = _passwordTextBox.Text;

            _settings.Betriebsmittel1Topic = _topic1TextBox.Text;
            _settings.Betriebsmittel2Topic = _topic2TextBox.Text;
            _settings.Betriebsmittel3Topic = _topic3TextBox.Text;
            _settings.Betriebsmittel4Topic = _topic4TextBox.Text;

            _settings.MotorNumber = _motorNumberTextBox.Text.Trim();
            _settings.Quitk = _quitkTextBox.Text.Trim();
            _settings.Tv = _tvTextBox.Text.Trim();
            _settings.Ma = _maTextBox.Text.Trim();
            _settings.Bauart = _bauartTextBox.Text.Trim();
            _settings.ToolPosition = _toolPositionTextBox.Text.Trim();
            _settings.ConnectTimeout = _connectTimeoutTextBox.Text.Trim();
            _settings.Dmc = _dmcTextBox.Text.Trim();

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