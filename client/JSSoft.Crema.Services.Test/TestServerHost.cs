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
        private AnonymousPipeServerStream pipeStream;
        private AnonymousPipeServerStream commandStream;

        public TestServerHost()
        {
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.ErrorDataReceived += Process_ErrorDataReceived;
            this.process.Exited += Process_Exited;
        }

        public void Start()
        {
            this.pipeStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            this.commandStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            this.errorBuilder.Clear();
            this.manualEvent.Reset();
            this.process.StartInfo.FileName = @"dotnet";
            this.process.StartInfo.Arguments = $"\"{this.ExecutablePath}\" test \"{this.RepositoryPath}\" --port {this.Port} --separator {this.id} --pipe-in {this.pipeStream.GetClientHandleAsString()} --pipe-out {this.commandStream.GetClientHandleAsString()}";
            this.process.StartInfo.WorkingDirectory = this.WorkingPath;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.Start();
            this.pipeStream.DisposeLocalCopyOfClientHandle();
            this.IsOpen = true;
            var sr = new StreamReader(this.pipeStream);
            var text = sr.ReadLine();

            if (this.process.HasExited == true)
            {
                throw new Exception(this.errorBuilder.ToString());
            }
        }

        public void Stop()
        {
            var sw = new StreamWriter(this.commandStream);
            sw.WriteLine("exit");
            sw.Close();
            this.commandStream.WaitForPipeDrain();
            this.process.WaitForExit();
            this.process.Kill();
            this.IsOpen = false;
            this.commandStream.Close();
            this.pipeStream.Close();
        }

        public string ExecutablePath { get; set; }

        public string RepositoryPath { get; set; }

        public int Port { get; set; }

        public string WorkingPath { get; set; }

        public bool IsOpen { get; set; }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.manualEvent.Set();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == $"{this.id}")
            {
                this.manualEvent.Set();
            }
            else
            {
                this.outputBuilder.AppendLine(e.Data);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.errorBuilder.AppendLine(e.Data);
        }
    }
}
