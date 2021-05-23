#if CLIENT
using System;
using System.Collections.Generic;
using System.Text;

namespace JSSoft.Crema.Services.Test
{
    class ServerHost
    {
        private readonly System.Threading.ManualResetEvent manualEvent = new(false);
        private readonly System.Diagnostics.Process process = new();
        private readonly System.Text.StringBuilder errorList = new();
        private Guid id = Guid.NewGuid();

        public string ExecutablePath { get; set; }

        public string RepositoryPath { get; set; }

        public int Port { get; set; }

        public string WorkingPath { get; set; }

        public ServerHost()
        {
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.ErrorDataReceived += Process_ErrorDataReceived;
            this.process.Exited += Process_Exited;
        }

        public void Start()
        {
            this.errorList.Clear();
            this.manualEvent.Reset();
            this.process.StartInfo.FileName = this.ExecutablePath;
            this.process.StartInfo.Arguments = $"run \"{this.RepositoryPath}\" --port {this.Port} --startup-message {this.id}";
            this.process.StartInfo.WorkingDirectory = this.WorkingPath;
            this.process.StartInfo.RedirectStandardInput = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.StartInfo.CreateNoWindow = true;
            this.process.EnableRaisingEvents = true;
            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();
            this.manualEvent.WaitOne();
            if (this.process.HasExited == true)
            {
                throw new Exception(this.errorList.ToString());
            }
        }

        public void Stop()
        {
            this.process.StandardInput.Flush();
            this.process.StandardInput.WriteLine("exit");
            this.process.WaitForExit();
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.manualEvent.Set();
        }

        private void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e.Data == $"{this.id}")
            {
                this.manualEvent.Set();
            }
        }

        private void Process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            this.errorList.AppendLine(e.Data);
        }
    }

}
#endif