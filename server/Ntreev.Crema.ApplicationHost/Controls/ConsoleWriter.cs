using Ntreev.Library.Commands;
using Ntreev.ModernUI.Framework.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ntreev.Crema.ApplicationHost.Controls
{
    class ConsoleWriter : StringWriter
    {
        private readonly TerminalControl control;

        public ConsoleWriter(TerminalControl control)
        {
            this.control = control;
            TerminalColor.ForegroundColorChanged += TerminalColor_ForegroundColorChanged;
            TerminalColor.BackgroundColorChanged += TerminalColor_BackgroundColorChanged;
        }

        public override void Write(char value)
        {
            base.Write(value);
            this.control.Dispatcher.Invoke(() => this.control.Append(value.ToString()));
        }

        public override void WriteLine()
        {
            base.WriteLine();
            this.control.Dispatcher.Invoke(() => this.control.AppendLine(string.Empty));
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            this.control.Dispatcher.Invoke(() => this.control.AppendLine(value));
        }

        public override void Write(string value)
        {
            base.Write(value);
            this.control.Dispatcher.Invoke(() => this.control.Append(value));
        }

        private void TerminalColor_ForegroundColorChanged(object sender, EventArgs e)
        {
            var foregroundColor = TerminalColor.ForegroundColor;

            this.control.Dispatcher.Invoke(() =>
            {
                if (foregroundColor == null)
                    this.control.OutputForeground = null;
                else
                    this.control.OutputForeground = (Brush)this.control.FindResource(TerminalColors.FindForegroundKey(foregroundColor));
            });
        }

        private void TerminalColor_BackgroundColorChanged(object sender, EventArgs e)
        {
            var backgroundColor = TerminalColor.BackgroundColor;
            this.control.Dispatcher.Invoke(() =>
            {
                if (backgroundColor == null)
                    this.control.OutputBackground = null;
                else
                    this.control.OutputBackground = (Brush)this.control.FindResource(TerminalColors.FindBackgroundKey(backgroundColor));
            });
        }
    }
}
