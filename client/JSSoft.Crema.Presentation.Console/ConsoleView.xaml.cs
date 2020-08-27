//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using JSSoft.Crema.Javascript;
using JSSoft.Crema.Presentation.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JSSoft.Crema.Presentation.Console
{
    /// <summary>
    /// ConsoleView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export]
    public partial class ConsoleView : UserControl
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly ConsoleCommandContext commandContext;
        private readonly ScriptContext scriptContext;

        public ConsoleView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public ConsoleView(ICremaAppHost cremaAppHost, ConsoleCommandContext commandContext, ScriptContext scriptContext)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.commandContext = commandContext;
            this.commandContext.PathChanged += CommandContext_PathChanged;
            this.commandContext.Executed += CommandContext_Executed;
            this.scriptContext = scriptContext;

            InitializeComponent();

            this.commandContext.Out = this.scriptContext.Out = new ConsoleWriter(this.terminal);
            this.terminal.CommandContext = this.commandContext;
            this.SetPrompt();
        }

        public async void Run(string commandLine)
        {
            try
            {
                await Task.Run(() => this.commandContext.Execute(this.commandContext.Name + " " + commandLine));
                this.SetPrompt();
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                if (e.InnerException != null)
                    this.terminal.AppendLine(e.InnerException.Message);
                else
                    this.terminal.AppendLine(e.Message);
            }
            catch (Exception e)
            {
                this.terminal.AppendLine(e.Message);
            }
            finally
            {
                this.terminal.InsertPrompt();
            }
        }

        public void Reset()
        {
            this.terminal.Reset();
        }

        protected void SetPrompt()
        {
            this.Dispatcher.InvokeAsync(() => this.terminal.Prompt = $"{this.commandContext.Prompt}> ");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.terminal.Focus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {


        }

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {
            this.SetPrompt();

            this.terminal.AppendLine(Properties.Resources.Comment_Hello);
            this.terminal.AppendLine(Properties.Resources.Comment_AvaliableCommands);
            foreach (var item in this.commandContext.Node.Commands)
            {
                if (item.IsEnabled == false)
                    continue;
                this.terminal.AppendLine(" - " + item.Name);
            }
            this.terminal.AppendLine(Properties.Resources.Comment_TypeHelp + Environment.NewLine);
            this.terminal.AppendLine(Properties.Resources.Comment_TypeVersion);
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.terminal.Reset();
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {

        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {

        }

        private void CommandContext_Executed(object sender, EventArgs e)
        {
            //this.SetPrompt();
        }

        private void CommandContext_PathChanged(object sender, EventArgs e)
        {
            //this.SetPrompt();
        }

        private void Term_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void TerminalControl_Executed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.Run(this.terminal.Text);
        }
    }
}
