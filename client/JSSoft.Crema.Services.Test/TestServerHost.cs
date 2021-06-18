using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public string ExecutablePath { get; set; }

        public string RepositoryPath { get; set; }

        public int Port { get; set; }

        public string WorkingPath { get; set; }

        public TestServerHost()
        {
            this.process.OutputDataReceived += Process_OutputDataReceived;
            this.process.ErrorDataReceived += Process_ErrorDataReceived;
            this.process.Exited += Process_Exited;
        }

        public void Start()
        {
            this.errorBuilder.Clear();
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
            this.ParseUsers();
            if (this.process.HasExited == true)
            {
                throw new Exception(this.errorBuilder.ToString());
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

        private void ParseUsers()
        {
            var text = this.outputBuilder.ToString();
            var jsonSer = new DataContractJsonSerializer(typeof(UserContextMetaData));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            var metaData = (UserContextMetaData)jsonSer.ReadObject(stream);

            int qwer = 0;
            // var capacity = text.Where(item => item == '\n').Count();
            // using var sr = new StringReader(text);

            // this.userList.Clear();
            // this.userList.Capacity = capacity;
            // while (sr.ReadLine() is string line)
            // {
            //     var match = Regex.Match(line, "(.+): (.+)");
            //     var userID = match.Groups[1].Value;
            //     var authority = (Authority)Enum.Parse(typeof(Authority), match.Groups[2].Value);
            //     this.userList.Add((userID, authority));
            // }
        }
    }
}
