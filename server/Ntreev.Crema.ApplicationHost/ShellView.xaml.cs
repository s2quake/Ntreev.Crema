﻿using FirstFloor.ModernUI.Windows.Controls;
using Ntreev.Crema.ApplicationHost.Commands.Consoles;
using Ntreev.Crema.ApplicationHost.Controls;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ntreev.Crema.ApplicationHost
{
    /// <summary>
    /// ShellView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export]
    public partial class ShellView : ModernWindow
    {
        private static LogTextWriter writer = new LogTextWriter();

        private readonly ICremaHost cremaHost;
        private readonly ConsoleCommandContext commandContext;

        static ShellView()
        {
            CremaLog.AddRedirection(writer, LogVerbose.Info);
        }

        public ShellView()
        {
            InitializeComponent();

        }

        [ImportingConstructor]
        public ShellView(ICremaHost cremaHost, ConsoleCommandContext commandContext)
        {
            InitializeComponent();
            writer.TextBox = this.logBox;
            this.cremaHost = cremaHost;
            this.cremaHost.Opening += CremaHost_Opening;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
            this.commandContext = commandContext;
            this.commandContext.Out = new ConsoleWriter(this.terminal);
            this.terminal.CommandContext = this.commandContext;
            this.SetPrompt();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.terminal.ApplyTemplate();
        }

        private readonly static string[] emptyStrings = new string[] { };

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

        private void CremaHost_Opening(object sender, EventArgs e)
        {
            if (this.cremaHost.GetService(typeof(ILogService)) is ILogService logService)
            {
                logService.AddRedirection(writer, LogVerbose.Info);
            }
        }

        private void LogBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void LogBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void TerminalControl_Executed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.Run(this.terminal.Text);
        }

        private void CremaHost_Opened(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.SetPrompt();

                this.terminal.AppendLine(Properties.Resources.Comment_Hello);
                this.terminal.AppendLine(Properties.Resources.Comment_AvaliableCommands);

                foreach (var item in this.commandContext.Commands)
                {
                    if (this.commandContext.IsCommandEnabled(item) == false)
                        continue;
                    this.terminal.AppendLine(" - " + item.Name);
                }
                this.terminal.AppendLine(Properties.Resources.Comment_TypeHelp + Environment.NewLine);
                this.terminal.AppendLine(Properties.Resources.Comment_TypeVersion);
            });
        }

        private void CremaHost_Closed(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                //this.terminal.Reset();
            });
        }

        private void OpenService_Click(object sender, RoutedEventArgs e)
        {
            this.logBox.Focus();
        }
    }
}