using System;
using System.Windows.Forms;
using BetriebsmittelPublisher.UI;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Logger.Initialize();
                Logger.Info("Anwendung wird gestartet...");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Logger.Info("FontManager wird initialisiert...");
                FontManager.Initialize();

                Logger.Info("MainForm wird erstellt...");
                Application.Run(new MainForm());

                Logger.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Error("Schwerwiegender Fehler beim Start", ex);
                MessageBox.Show(
                    $"Ein schwerwiegender Fehler ist aufgetreten:\n\n{ex.Message}\n\nLog-Datei: {Logger.GetCurrentLogFile()}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}