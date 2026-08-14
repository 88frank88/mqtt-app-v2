using System;
using System.Windows.Forms;
using System.Drawing;
using BetriebsmittelPublisher.Models;
using BetriebsmittelPublisher.Services;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.UI
{
    public class AutomationWindow : BaseForm
    {
        private TextBox _motorNumberTextBox = null!;
        private Label _motorNumberValidationLabel = null!;
        private DataGridView _dataGrid = null!;
        private Button _addRowButton = null!;
        private Button _removeRowButton = null!;
        private Button _publishButton = null!;
        private Button _clearButton = null!;
        private Label _connectionStatus = null!;
        private readonly Services.ConnectionManager _connectionManager;
        private readonly XmlConverter _xmlConverter = new XmlConverter();
        private Models.SettingsModel _settings = null!;
        private const int MaxRows = 10;

        public AutomationWindow(Services.ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _settings = SettingsPersistence.Load();
            Text = $"PG-Number Automation - {Core.VersionInfo.ShortInfo}";
            InitializeComponents();
            Logger.Info("AutomationWindow geoeffnet");
        }

        private void InitializeComponents()
        {
            this.Size = new Size(950, 720);
            this.BackColor = DesignSystem.Colors.WindowBackground;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(DesignSystem.Spacing.Large),
                BackColor = DesignSystem.Colors.WindowBackground
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

            layout.Controls.Add(CreateHeaderSection(), 0, 0);
            layout.Controls.Add(CreateMotorNumberSection(), 0, 1);
            layout.Controls.Add(CreateTableSection(), 0, 2);
            layout.Controls.Add(CreateActionSection(), 0, 3);

            this.Controls.Add(layout);
        }

        private Panel CreateHeaderSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.Colors.WindowBackground
            };

            var title = new Label
            {
                Text = "Betriebsmittel-Publisher",
                Font = DesignSystem.Fonts.Headline,
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _connectionStatus = new Label
            {
                Text = "Getrennt",
                Font = DesignSystem.Fonts.Caption,
                ForeColor = DesignSystem.Colors.Error,
                Dock = DockStyle.Right,
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 0)
            };

            var disconnectButton = new Button
            {
                Text = "Trennen",
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Right,
                Margin = new Padding(8, 15, 8, 0)
            };
            disconnectButton.FlatAppearance.BorderSize = 1;
            disconnectButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;
            disconnectButton.Click += DisconnectButton_Click;

            var connectButton = new Button
            {
                Text = "Verbinden",
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DesignSystem.Colors.Accent,
                ForeColor = DesignSystem.Colors.ButtonForeground,
                Dock = DockStyle.Right,
                Margin = new Padding(8, 15, 8, 0)
            };
            connectButton.Click += ConnectButton_Click;

            panel.Controls.AddRange(new Control[] { title, _connectionStatus, disconnectButton, connectButton });
            UpdateConnectionStatus();
            return panel;
        }

        private Panel CreateMotorNumberSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.Colors.WindowBackground
            };

            var label = new Label
            {
                Text = "Motor-Nummer (8-stellig):",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.TextPrimary,
                Location = new Point(0, 10),
                AutoSize = true
            };

            _motorNumberTextBox = new TextBox
            {
                Font = DesignSystem.Fonts.Monospace,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BackColor = DesignSystem.Colors.ControlBackground,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(190, 7),
                Size = new Size(160, 28),
                MaxLength = 8,
                Text = _settings.MotorNumber
            };
            _motorNumberTextBox.TextChanged += MotorNumberTextBox_TextChanged;

            _motorNumberValidationLabel = new Label
            {
                Text = "",
                Font = DesignSystem.Fonts.Caption,
                ForeColor = DesignSystem.Colors.Error,
                Location = new Point(365, 12),
                AutoSize = true
            };

            panel.Controls.AddRange(new Control[] { label, _motorNumberTextBox, _motorNumberValidationLabel });
            ValidateMotorNumber();
            return panel;
        }

        private void MotorNumberTextBox_TextChanged(object? sender, EventArgs e)
        {
            ValidateMotorNumber();
        }

        private bool ValidateMotorNumber()
        {
            var text = _motorNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                _motorNumberValidationLabel.Text = "Pflichtfeld";
                _motorNumberValidationLabel.ForeColor = DesignSystem.Colors.Error;
                return false;
            }

            if (text.Length != 8 || !long.TryParse(text, out _))
            {
                _motorNumberValidationLabel.Text = "Genau 8 Ziffern erforderlich";
                _motorNumberValidationLabel.ForeColor = DesignSystem.Colors.Error;
                return false;
            }

            _motorNumberValidationLabel.Text = "OK";
            _motorNumberValidationLabel.ForeColor = Color.FromArgb(76, 175, 80);
            return true;
        }

        private Panel CreateTableSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.Colors.WindowBackground
            };

            _dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = DesignSystem.Colors.WindowBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                GridColor = DesignSystem.Colors.ControlBorder,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EditMode = DataGridViewEditMode.EditOnEnter
            };

            var betriebsmittelColumn = new DataGridViewComboBoxColumn
            {
                Name = "Betriebsmittel",
                HeaderText = "Betriebsmittel",
                ReadOnly = false,
                FillWeight = 30
            };

            betriebsmittelColumn.Items.Add("Betriebsmittel 1");
            betriebsmittelColumn.Items.Add("Betriebsmittel 2");
            betriebsmittelColumn.Items.Add("Betriebsmittel 3");
            betriebsmittelColumn.Items.Add("Betriebsmittel 4");

            var pgColumn = new DataGridViewTextBoxColumn
            {
                Name = "PgNumber",
                HeaderText = "PG-Nummer",
                ReadOnly = false,
                FillWeight = 40
            };

            var statusColumn = new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                ReadOnly = true,
                FillWeight = 30
            };

            _dataGrid.Columns.Add(betriebsmittelColumn);
            _dataGrid.Columns.Add(pgColumn);
            _dataGrid.Columns.Add(statusColumn);

            _dataGrid.DefaultCellStyle.BackColor = DesignSystem.Colors.ControlBackground;
            _dataGrid.DefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
            _dataGrid.DefaultCellStyle.SelectionBackColor = DesignSystem.Colors.ControlHover;
            _dataGrid.DefaultCellStyle.SelectionForeColor = DesignSystem.Colors.TextPrimary;
            _dataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(0x26, 0x2a, 0x3a);
            _dataGrid.AlternatingRowsDefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
            _dataGrid.ColumnHeadersDefaultCellStyle.BackColor = DesignSystem.Colors.ControlBackground;
            _dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
            _dataGrid.EnableHeadersVisualStyles = false;
            _dataGrid.RowTemplate.Height = 30;

            _dataGrid.DataError += (s, e) => { e.ThrowException = false; };

            panel.Controls.Add(_dataGrid);

            var rowButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 40,
                Padding = new Padding(0, 5, 0, 0)
            };

            _addRowButton = new Button
            {
                Text = "+ Zeile hinzufuegen",
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary
            };
            _addRowButton.FlatAppearance.BorderSize = 1;
            _addRowButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;
            _addRowButton.Click += (s, e) => AddRow();
            rowButtonPanel.Controls.Add(_addRowButton);

            _removeRowButton = new Button
            {
                Text = "- Zeile entfernen",
                Size = new Size(140, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary
            };
            _removeRowButton.FlatAppearance.BorderSize = 1;
            _removeRowButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;
            _removeRowButton.Click += (s, e) => RemoveSelectedRow();
            rowButtonPanel.Controls.Add(_removeRowButton);

            var hintLabel = new Label
            {
                Text = "Publish veroeffentlicht nur die markierte Zeile",
                Font = DesignSystem.Fonts.Caption,
                ForeColor = DesignSystem.Colors.TextSecondary,
                AutoSize = true,
                Padding = new Padding(10, 7, 0, 0)
            };
            rowButtonPanel.Controls.Add(hintLabel);

            panel.Controls.Add(rowButtonPanel);

            AddRow();

            return panel;
        }

        private void AddRow()
        {
            if (_dataGrid.Rows.Count >= MaxRows)
            {
                Logger.Warning($"Maximale Zeilenzahl ({MaxRows}) erreicht");
                return;
            }

            var rowIndex = _dataGrid.Rows.Add();
            var row = _dataGrid.Rows[rowIndex];
            row.Cells["Betriebsmittel"].Value = "Betriebsmittel 1";
            row.Cells["Status"].Value = "offen";
            Logger.Debug($"Zeile {rowIndex + 1} hinzugefuegt");
        }

        private void RemoveSelectedRow()
        {
            if (_dataGrid.CurrentRow != null && !_dataGrid.CurrentRow.IsNewRow)
            {
                _dataGrid.Rows.Remove(_dataGrid.CurrentRow);
            }
        }

        private Panel CreateActionSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.Colors.WindowBackground
            };

            _publishButton = new Button
            {
                Text = "Publish markierte Zeile",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.ButtonForeground,
                BackColor = DesignSystem.Colors.Accent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(210, 40),
                Location = new Point(690, 10)
            };
            _publishButton.FlatAppearance.BorderSize = 0;
            _publishButton.Click += PublishButton_Click;

            _clearButton = new Button
            {
                Text = "Tabelle leeren",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BackColor = DesignSystem.Colors.ControlBackground,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 40),
                Location = new Point(530, 10)
            };
            _clearButton.FlatAppearance.BorderSize = 1;
            _clearButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;
            _clearButton.Click += ClearButton_Click;

            panel.Controls.AddRange(new Control[] { _clearButton, _publishButton });
            return panel;
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _settings = SettingsPersistence.Load();
                Logger.Info($"Verbinde mit Broker {_settings.BrokerAddress}:{_settings.BrokerPort}...");

                _connectionStatus.Text = "Verbinde...";
                _connectionStatus.ForeColor = DesignSystem.Colors.Warning;

                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _connectionManager.ConnectAsync(_settings.BrokerAddress, _settings.BrokerPort, cts.Token);

                var connectMessage = new MqttConnectMessage
                {
                    ClientId = string.IsNullOrWhiteSpace(_settings.ClientId) ? "BetriebsmittelPublisher" : _settings.ClientId,
                    Username = _settings.Username,
                    Password = _settings.Password
                };

                var connectPacket = MqttPacketBuilder.BuildConnectPacket(connectMessage);
                await _connectionManager.SendPacketAsync(connectPacket, cts.Token);
                var response = await _connectionManager.ReceivePacketAsync(cts.Token);

                Logger.Info($"CONNACK erhalten: {BitConverter.ToString(response)}");

                bool accepted = response.Length >= 4 && response[0] == 0x20 && response[3] == 0x00;
                if (!accepted)
                {
                    byte returnCode = response.Length >= 4 ? response[3] : (byte)0xFF;
                    Logger.Warning($"Broker lehnte Verbindung ab (Code {returnCode})");
                    await _connectionManager.DisconnectAsync();
                    UpdateConnectionStatus();
                    MessageBox.Show($"Broker lehnte die Verbindung ab (Return-Code {returnCode}).\nPruefen Sie Client-ID und Zugangsdaten.", "Verbindung abgelehnt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Logger.Info("MQTT-Verbindung hergestellt");
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                Logger.Error("Verbindung fehlgeschlagen", ex);
                UpdateConnectionStatus();
                MessageBox.Show($"Verbindung fehlgeschlagen: {ex.Message}\n\nPruefen Sie die Broker-Einstellungen unter 'Settings'.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DisconnectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_connectionManager.IsConnected)
                {
                    await _connectionManager.DisconnectAsync();
                    Logger.Info("MQTT-Verbindung getrennt");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Trennen fehlgeschlagen", ex);
            }
            UpdateConnectionStatus();
        }

        private void UpdateConnectionStatus()
        {
            if (_connectionManager != null && _connectionManager.IsConnected)
            {
                _connectionStatus.Text = "Verbunden";
                _connectionStatus.ForeColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                _connectionStatus.Text = "Getrennt";
                _connectionStatus.ForeColor = DesignSystem.Colors.Error;
            }
        }

        private void PublishButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_dataGrid.IsCurrentCellInEditMode)
                    _dataGrid.EndEdit();

                if (!_connectionManager.IsConnected)
                {
                    Logger.Warning("Publish-Versuch ohne MQTT-Verbindung");
                    MessageBox.Show("Nicht mit MQTT-Broker verbunden. Bitte zuerst verbinden.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var row = _dataGrid.CurrentRow;
                if (row == null || row.IsNewRow)
                {
                    MessageBox.Show("Bitte zuerst eine Zeile markieren.", "Keine Zeile markiert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var motorNumber = _motorNumberTextBox.Text.Trim();
                if (!ValidateMotorNumber())
                {
                    MessageBox.Show("Motor-Nummer ungueltig: genau 8 Ziffern erforderlich.", "Validierung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var betriebsmittel = row.Cells["Betriebsmittel"].Value?.ToString() ?? "";
                var pgNumber = row.Cells["PgNumber"].Value?.ToString()?.Trim() ?? "";

                if (string.IsNullOrEmpty(betriebsmittel))
                {
                    MessageBox.Show($"Zeile {row.Index + 1}: Bitte Betriebsmittel auswaehlen.", "Validierung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(pgNumber))
                {
                    MessageBox.Show($"Zeile {row.Index + 1}: Bitte PG-Nummer eintragen.", "Validierung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _settings = SettingsPersistence.Load();

                var topicMap = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Betriebsmittel 1", _settings.Betriebsmittel1Topic },
                    { "Betriebsmittel 2", _settings.Betriebsmittel2Topic },
                    { "Betriebsmittel 3", _settings.Betriebsmittel3Topic },
                    { "Betriebsmittel 4", _settings.Betriebsmittel4Topic }
                };

                if (!topicMap.TryGetValue(betriebsmittel, out var topic) || string.IsNullOrWhiteSpace(topic))
                {
                    row.Cells["Status"].Value = "kein Topic";
                    MessageBox.Show($"Kein Topic konfiguriert fuer {betriebsmittel}.\nBitte in den Einstellungen eintragen.", "Kein Topic", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var modul = _settings.StationNumber.Trim();
                if (string.IsNullOrEmpty(modul))
                {
                    MessageBox.Show("Keine Station-Nummer (Modul) konfiguriert.\nBitte in den Einstellungen unter 'Automation Parameter' eintragen.", "Keine Station", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Motor-Nummer in Settings zurueckspeichern (global konfigurierbar)
                if (_settings.MotorNumber != motorNumber)
                {
                    _settings.MotorNumber = motorNumber;
                    SettingsPersistence.Save(_settings);
                }

                var xmlData = new ExecutionXmlData
                {
                    PgNumber = pgNumber,
                    Topic = topic,
                    MotorNumber = motorNumber,
                    Modul = modul,
                    Host = _settings.BrokerAddress,
                    Port = _settings.BrokerPort.ToString(),
                    Quitk = _settings.Quitk,
                    Tv = _settings.Tv,
                    Ma = _settings.Ma,
                    Bauart = _settings.Bauart,
                    ToolPosition = _settings.ToolPosition,
                    ConnectTimeout = _settings.ConnectTimeout,
                    Dmc = _settings.Dmc
                };

                var xml = _xmlConverter.GenerateExecutionXml(xmlData);
                Logger.Info($"TExecution-XML generiert (modul={modul}, feature={pgNumber}, motorNr={motorNumber}):{Environment.NewLine}{xml}");

                var message = new MqttMessage
                {
                    Topic = topic,
                    Payload = System.Text.Encoding.UTF8.GetBytes(xml),
                    QoS = MqttQoS.AtMostOnce
                };

                _connectionManager.Publish(message);
                row.Cells["Status"].Value = "veroeffentlicht";

                Logger.Info($"Zeile {row.Index + 1} veroeffentlicht: {betriebsmittel} -> {topic}");
                MessageBox.Show($"Zeile {row.Index + 1} erfolgreich veroeffentlicht:{Environment.NewLine}{betriebsmittel} -> {topic}{Environment.NewLine}PG: {pgNumber} | Modul: {modul}", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Fehler beim Veroeffentlichen", ex);
                if (_dataGrid.CurrentRow != null)
                    _dataGrid.CurrentRow.Cells["Status"].Value = "Fehler";
                MessageBox.Show($"Fehler beim Veroeffentlichen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            _dataGrid.Rows.Clear();
            AddRow();
            Logger.Info("Tabelle geleert");
        }
    }
}