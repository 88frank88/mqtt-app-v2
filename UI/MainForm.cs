using System;
using System.Drawing;
using System.Windows.Forms;
using BetriebsmittelPublisher.Services;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.UI
{
    public class MainForm : BaseForm
    {
        private Button _settingsButton;
        private Button _automationButton;
        private AutomationWindow _automationWindow;
        private ConnectionManager _connectionManager;

        public MainForm()
        {
            Text = Core.VersionInfo.ShortInfo;
            Size = new Size(800, 600);
            MinimumSize = new Size(600, 400);
            _connectionManager = new ConnectionManager();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _settingsButton = new Button
            {
                Text = "Settings",
                Size = new Size(100, 30),
                Location = new Point(20, 20),
                BackColor = DesignSystem.Colors.ControlBackground,
                ForeColor = DesignSystem.Colors.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            _settingsButton.Click += SettingsButton_Click;
            Controls.Add(_settingsButton);

            _automationButton = new Button
            {
                Text = "PG Automation",
                Size = new Size(120, 30),
                Location = new Point(140, 20),
                BackColor = DesignSystem.Colors.Accent,
                ForeColor = DesignSystem.Colors.ButtonForeground,
                FlatStyle = FlatStyle.Flat
            };
            _automationButton.Click += AutomationButton_Click;
            Controls.Add(_automationButton);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (var settingsWindow = new SettingsWindow())
            {
                settingsWindow.ShowDialog(this);
            }
        }

        private void AutomationButton_Click(object sender, EventArgs e)
        {
            if (_automationWindow == null || _automationWindow.IsDisposed)
            {
                _automationWindow = new AutomationWindow(_connectionManager);
            }

            if (_automationWindow.Visible)
            {
                _automationWindow.BringToFront();
            }
            else
            {
                _automationWindow.Show(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}