using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSSoft.Crema.Services.Test
{
    class TestServerHost
    {
        private readonly ManualResetEvent manualEvent = new(false);
        private readonly Process process = new();
        private readonly StringBuilder errorBuilder = new();
        private readonly StringBuilder outputBuilder = new();
        private readonly List<(string, Authority)> userList = new();
        private Guid id = Guid.NewGuid();
        private AnonymousPipeServerStream inputStream;
        private AnonymousPipeServerStream outputStream;
        private AnonymousPipeServerStream errorStream;
        private CancellationTokenSource cancellation;
        private Task outputTask;
        private Task errorTask;
        private StreamWriter inputWriter;

        public TestServerHost()
        {
        }

        public async Task StartAsync()
        {
            this.cancellation = new();
            this.inputStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            this.outputStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            this.errorStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            this.inputWriter = new StreamWriter(inputStream) { AutoFlush = true };
            this.errorBuilder.Clear();
            this.manualEvent.Reset();
            this.process.StartInfo.FileName = @"dotnet";
            this.process.StartInfo.Arguments = $"\"{this.ExecutablePath}\" test \"{this.RepositoryPath}\" --port {this.Port} --separator {this.id} --pipe-input {this.inputStream.GetClientHandleAsString()} --pipe-output {this.outputStream.GetClientHandleAsString()} --pipe-error {this.errorStream.GetClientHandleAsString()}";
            this.process.StartInfo.WorkingDirectory = this.WorkingPath;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.CreateNoWindow = true;
            this.outputTask = ReadStreamAsync(outputStream, this.OnOutputMessage, this.cancellation.Token);
            this.errorTask = ReadStreamAsync(errorStream, this.OnErrorMessage, this.cancellation.Token);
            this.process.Start();
            this.inputStream.DisposeLocalCopyOfClientHandle();
            this.IsOpen = true;
            this.manualEvent.WaitOne();

            if (this.process.HasExited == true)
            {
                throw new Exception(this.errorBuilder.ToString());
            }
        }

        public async Task StopAsync()
        {
            this.cancellation.Cancel();
            await this.inputWriter.WriteLineAsync("exit");
            await Task.WhenAll(this.outputTask, this.errorTask);
            this.process.WaitForExit();
            this.process.Kill();
            this.IsOpen = false;
            this.inputWriter.Close();
            this.errorStream.Close();
            this.outputStream.Close();
            this.inputStream.Close();
        }

        public string ExecutablePath { get; set; }

        public string RepositoryPath { get; set; }

        public int Port { get; set; }

        public string WorkingPath { get; set; }

        public bool IsOpen { get; set; }

        private void OnOutputMessage(string text)
        {
            if (text == $"{this.id}")
            {
                this.manualEvent.Set();
            }
        }

        private void OnErrorMessage(string text)
        {

        }

        private static async Task ReadStreamAsync(AnonymousPipeServerStream stream, Action<string> action, CancellationToken cancellation)
        {
            await Task.Run(() =>
            {
                using var reader = new StreamReader(stream);
                while (cancellation.IsCancellationRequested == false && reader.ReadLine() is string line)
                {
                    action(line);
                }
            });
        }
    }
}
