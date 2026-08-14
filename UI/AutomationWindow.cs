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
        private PgAutomationModel _model = null!;
        private PgNumberGenerator _generator = null!;
        private XmlConverter _xmlConverter = null!;
        private TextBox _motorNumberInput = null!;
        private DataGridView _dataGrid = null!;
        private Button _generateButton = null!;
        private Button _publishButton = null!;
        private Button _clearButton = null!;
        private Label _connectionStatus = null!;
        private readonly Services.ConnectionManager _connectionManager;
        private Models.SettingsModel _settings = null!;

        public AutomationWindow(Services.ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _model = new PgAutomationModel();
            _generator = new PgNumberGenerator();
            _xmlConverter = new XmlConverter();
            _settings = SettingsPersistence.Load();
            Text = $"PG-Number Automation - {Core.VersionInfo.ShortInfo}";
            InitializeComponents();
            SubscribeToEvents();
            Logger.Info("AutomationWindow geoeffnet");
        }

        private void InitializeComponents()
        {
            this.Text = "PG-Number Automation";
            this.Size = new Size(900, 700);
            this.BackColor = DesignSystem.Colors.WindowBackground;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(DesignSystem.Spacing.Large),
                BackColor = DesignSystem.Colors.WindowBackground
            };

            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

            layout.Controls.Add(CreateHeaderSection(), 0, 0);
            layout.Controls.Add(CreateInputSection(), 0, 1);
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
                Text = "PG-Number Automation",
                Font = DesignSystem.Fonts.Headline,
                ForeColor = DesignSystem.Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _connectionStatus = new Label
            {
                Text = "Disconnected",
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
            return panel;
        }

        private Panel CreateInputSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, DesignSystem.Spacing.Medium, 0, 0),
                BackColor = DesignSystem.Colors.WindowBackground
            };

            var label = new Label
            {
                Text = "Motor Number:",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.TextPrimary,
                Location = new Point(0, 5),
                AutoSize = true
            };

            _motorNumberInput = new TextBox
            {
                Font = DesignSystem.Fonts.Monospace,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BackColor = DesignSystem.Colors.ControlBackground,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(150, 5),
                Size = new Size(200, 30),
                Text = _model.MotorNumber
            };

            _motorNumberInput.TextChanged += (s, e) => 
            {
                _model.MotorNumber = _motorNumberInput.Text;
            };

            panel.Controls.AddRange(new Control[] { label, _motorNumberInput });
            return panel;
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
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = DesignSystem.Colors.WindowBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                GridColor = DesignSystem.Colors.ControlBorder,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };

            _dataGrid.CellFormatting += DataGrid_CellFormatting;
            _dataGrid.RowsAdded += (s, e) => UpdateTableRows();

            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Row",
                DataPropertyName = "RowNumber",
                Width = 50
            });

            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Motor Number",
                DataPropertyName = "MotorNumber",
                Width = 150
            });

            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "PG Number",
                DataPropertyName = "PgNumber",
                Width = 250
            });

            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Status",
                DataPropertyName = "Status",
                Width = 100
            });

            _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Actions",
                Width = 150
            });

            panel.Controls.Add(_dataGrid);
            return panel;
        }

        private Panel CreateActionSection()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DesignSystem.Colors.WindowBackground,
                Padding = new Padding(0, DesignSystem.Spacing.Medium, 0, 0)
            };

            _generateButton = new Button
            {
                Text = "Generate PG Numbers",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.ButtonForeground,
                BackColor = DesignSystem.Colors.Accent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 40),
                Location = new Point(600, 10)
            };

            _generateButton.FlatAppearance.BorderSize = 0;

            _publishButton = new Button
            {
                Text = "Publish XML",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BackColor = DesignSystem.Colors.ControlBackground,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(420, 10)
            };

            _publishButton.FlatAppearance.BorderSize = 1;
            _publishButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;

            _clearButton = new Button
            {
                Text = "Clear Table",
                Font = DesignSystem.Fonts.Body,
                ForeColor = DesignSystem.Colors.TextPrimary,
                BackColor = DesignSystem.Colors.ControlBackground,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Location = new Point(280, 10)
            };

            _clearButton.FlatAppearance.BorderSize = 1;
            _clearButton.FlatAppearance.BorderColor = DesignSystem.Colors.ControlBorder;

            _generateButton.Click += GenerateButton_Click;
            _publishButton.Click += PublishButton_Click;
            _clearButton.Click += ClearButton_Click;

            panel.Controls.AddRange(new Control[] { _clearButton, _publishButton, _generateButton });
            return panel;
        }

        private void SubscribeToEvents()
        {
            _model.PropertyChanged += Model_PropertyChanged;
            UpdateConnectionStatus();
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

                bool accepted = response.Length >= 4 && response[3] == 0x00;
                if (!accepted)
                {
                    byte returnCode = response.Length >= 4 ? response[3] : (byte)0xFF;
                    Logger.Warning($"Broker lehnte Verbindung ab (Code {returnCode})");
                    await _connectionManager.DisconnectAsync();
                    UpdateConnectionStatus();
                    MessageBox.Show($"Broker lehnte die Verbindung ab (Return-Code {returnCode}).\nPrüfen Sie Client-ID und Zugangsdaten.", "Verbindung abgelehnt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Logger.Info("MQTT-Verbindung hergestellt");
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                Logger.Error("Verbindung fehlgeschlagen", ex);
                UpdateConnectionStatus();
                MessageBox.Show($"Verbindung fehlgeschlagen: {ex.Message}\n\nPrüfen Sie die Broker-Einstellungen unter 'Settings'.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _connectionStatus.Text = "Connected";
                _connectionStatus.ForeColor = Color.FromArgb(76, 175, 80);
            }
            else
            {
                _connectionStatus.Text = "Disconnected";
                _connectionStatus.ForeColor = DesignSystem.Colors.Error;
            }
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PgAutomationModel.TableRows))
            {
                UpdateTableRows();
            }
        }

        private void UpdateTableRows()
        {
            _dataGrid.DataSource = null;
            _dataGrid.DataSource = _model.TableRows;
        }

        private void DataGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var row = _dataGrid.Rows[e.RowIndex];
                var dataRow = row.DataBoundItem as PgTableRow;

                if (dataRow != null)
                {
                    row.DefaultCellStyle.ForeColor = DesignSystem.Colors.TextPrimary;
                    row.DefaultCellStyle.BackColor = DesignSystem.Colors.WindowBackground;
                    row.DefaultCellStyle.SelectionForeColor = DesignSystem.Colors.TextPrimary;
                    row.DefaultCellStyle.SelectionBackColor = DesignSystem.Colors.Accent;

                    switch (dataRow.Status.ToLower())
                    {
                        case "generated":
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(76, 175, 80);
                            break;
                        case "published":
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(33, 150, 243);
                            break;
                        case "error":
                            row.DefaultCellStyle.ForeColor = DesignSystem.Colors.Error;
                            break;
                    }
                }
            }
        }

        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            if (!_model.ValidateMotorNumber(out string? error))
            {
                Logger.Warning($"Ungueltige Motor-Nummer: {_model.MotorNumber}");
                MessageBox.Show(error ?? "Invalid motor number", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int motorNumber = int.Parse(_model.MotorNumber);
            Logger.Info($"Generiere PG-Nummern fuer Motor: {motorNumber}");
            var pgNumbers = _generator.GeneratePgNumbers(motorNumber, 10);
            Logger.Info($"{pgNumbers.Length} PG-Nummern generiert");

            for (int i = 0; i < _model.TableRows.Count && i < pgNumbers.Length; i++)
            {
                _model.TableRows[i].MotorNumber = _model.MotorNumber;
                _model.TableRows[i].PgNumber = pgNumbers[i];
                _model.TableRows[i].Status = "generated";
                _model.TableRows[i].Timestamp = DateTime.Now;
                _model.TableRows[i].Error = string.Empty;
            }

            UpdateTableRows();
        }

        private void PublishButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var xml = _xmlConverter.GenerateXml(_model.TableRows);
                Logger.Info($"XML generiert ({xml.Length} bytes)");

                if (_connectionManager != null && _connectionManager.IsConnected)
                {
                    _settings = SettingsPersistence.Load();
                    var payload = System.Text.Encoding.UTF8.GetBytes(xml);

                    var topics = new[]
                    {
                        _settings.Betriebsmittel1Topic,
                        _settings.Betriebsmittel2Topic,
                        _settings.Betriebsmittel3Topic,
                        _settings.Betriebsmittel4Topic
                    };

                    int publishedCount = 0;
                    foreach (var topic in topics)
                    {
                        if (string.IsNullOrWhiteSpace(topic))
                            continue;

                        var message = new MqttMessage
                        {
                            Topic = topic,
                            Payload = payload,
                            QoS = MqttQoS.AtMostOnce
                        };

                        _connectionManager.Publish(message);
                        publishedCount++;
                    }

                    foreach (var row in _model.TableRows)
                    {
                        if (row.Status == "generated")
                        {
                            row.Status = "published";
                        }
                    }

                    UpdateTableRows();
                    Logger.Info($"XML erfolgreich auf {publishedCount} Topics veroeffentlicht");
                    MessageBox.Show($"XML erfolgreich auf {publishedCount} Topics veroeffentlicht", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Logger.Warning("Publish-Versuch ohne MQTT-Verbindung");
                    MessageBox.Show("Nicht mit MQTT-Broker verbunden. Bitte zuerst verbinden.", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Fehler beim Veroeffentlichen des XML", ex);
                MessageBox.Show($"Fehler beim Veroeffentlichen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            _model.ClearTable();
            _motorNumberInput.Text = string.Empty;
            UpdateTableRows();
        }
    }
}