using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            this.process.StartInfo.FileName = "dotnet";
            this.process.StartInfo.Arguments = $"\"{this.ExecutablePath}\" test \"{this.RepositoryPath}\" --port {this.Port} --separator {this.id}";
            this.process.StartInfo.WorkingDirectory = this.WorkingPath;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardInput = true;
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

        public async Task GenerateDataBasesAsync(int count)
        {
            this.manualEvent.Reset();
            this.process.StandardInput.Flush();
            this.process.StandardInput.WriteLine("database generate 10");
            await Task.Run(() => this.manualEvent.WaitOne());
        }

        public async Task LoginRandomManyAsync()
        {
            this.manualEvent.Reset();
            this.process.StandardInput.Flush();
            this.process.StandardInput.WriteLine("cremahost login-many");
            await Task.Run(() => this.manualEvent.WaitOne());
        }

        public async Task LoadRandomDataBasesAsync()
        {

        }

        public async Task LockRandomDataBasesAsync()
        {

        }

        public async Task SetPrivateRandomDataBasesAsync()
        {

        }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.manualEvent.Set();
        }

        private void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
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