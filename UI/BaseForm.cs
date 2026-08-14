using System.Drawing;
using System.Windows.Forms;
using BetriebsmittelPublisher.Core;

namespace BetriebsmittelPublisher.UI
{
    public class BaseForm : Form
    {
        public BaseForm()
        {
            ApplyDarkModeTheme();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var brush = new SolidBrush(DesignSystem.Colors.WindowBackground))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        protected void ApplyDarkModeTheme()
        {
            BackColor = DesignSystem.Colors.WindowBackground;
            ForeColor = DesignSystem.Colors.TextPrimary;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            ApplyThemeToControls(Controls);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyThemeToControl(control);
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        private void ApplyThemeToControl(Control control)
        {
            control.BackColor = DesignSystem.Colors.ControlBackground;
            control.ForeColor = DesignSystem.Colors.TextPrimary;
        }
    }
}