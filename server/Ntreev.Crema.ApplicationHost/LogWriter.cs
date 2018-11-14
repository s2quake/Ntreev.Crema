using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ntreev.Crema.ApplicationHost
{
    class LogWriter : TextWriter
    {
        private TextBox textBox;
        private readonly StringBuilder sb = new StringBuilder();

        public LogWriter()
        {

        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public TextBox TextBox
        {
            get { return this.textBox; }
            set
            {
                this.textBox = value;
                if (this.textBox != null && this.sb.Length > 0)
                {
                    this.textBox.AppendText(this.sb.ToString());
                    this.sb.Clear();
                }
            }
        }

        public override void Write(char value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, $"{value}");
            }
            else
            {
                this.sb.Append(value);
            }
        }

        public override void Write(string value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, value);
            }
            else
            {
                this.sb.Append(value);
            }
        }

        public override void WriteLine(string value)
        {
            if (this.textBox != null)
            {
                this.Redirect(this.textBox, value + Environment.NewLine);
            }
            else
            {
                this.sb.AppendLine(value);
            }
        }

        private async void Redirect(TextBox textBox, string value)
        {
            await textBox.Dispatcher.InvokeAsync(() =>
            {
                var isEnd = textBox.CaretIndex == textBox.Text.Length;
                textBox.AppendText(value);
                if (isEnd == true)
                    textBox.CaretIndex = textBox.Text.Length;
            });
        }
    }
}
