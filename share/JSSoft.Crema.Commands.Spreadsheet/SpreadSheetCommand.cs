// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Commands.Consoles;
using JSSoft.Crema.Commands.Consoles.Properties;
using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Spreadsheet;
using JSSoft.Library;
using JSSoft.Library.Commands;
using JSSoft.Library.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Spreadsheet
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources")]
    class SpreadSheetCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;

        private string dataBaseName;

        [ImportingConstructor]
        public SpreadSheetCommand(ICremaHost cremaHost)
            : base(GetName())
        {
            this.cremaHost = cremaHost;
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(DataSetTypeProperties))]
        [CommandMethodProperty(nameof(DataBaseName), nameof(OmitAttribute), nameof(OmitSignatureDate), nameof(Revision), nameof(SaveEach), nameof(IsForce))]
        public async Task ExportAsync(string filename)
        {
            this.ValidateExport(filename);
            var path = PathUtility.GetFullPath(filename, this.CommandContext.BaseDirectory);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataBase = this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext[this.DataBaseName]);
            var revision = dataBase.Dispatcher.Invoke(() => this.Revision ?? dataBase.DataBaseInfo.Revision);
            var dataSet = await dataBase.GetDataSetAsync(authentication, DataSetTypeProperties.DataSetType, FilterProperties.FilterExpression, revision);
            var settings = new SpreadsheetWriterSettings()
            {
                OmitAttribute = this.OmitAttribute,
                OmitSignatureDate = this.OmitSignatureDate,
                OmitType = DataSetTypeProperties.TableOnly,
                OmitTable = DataSetTypeProperties.TypeOnly,
            };
            if (this.SaveEach == true)
                settings.Sort = this.Comparison;
            settings.Properties.Add(nameof(Revision), revision);
            settings.Properties.Add(nameof(FilterProperties.FilterExpression), FilterProperties.FilterExpression);
            settings.Properties.Add(nameof(DataBaseName), this.DataBaseName);
            if (this.SaveEach == true)
                this.WriteDataSetToDirectory(dataSet, path, settings);
            else
                this.WriteDataSet(dataSet, path, settings);
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Message))]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        public async Task ImportAsync(string filename)
        {
            var path = PathUtility.GetFullPath(filename, this.CommandContext.BaseDirectory);
            var sheetNames = SpreadsheetReader.ReadTableNames(path);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataBase = this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext[this.DataBaseName]);
            var tableNames = dataBase.Dispatcher.Invoke(() => dataBase.TableContext.Tables.Select(item => item.Name).ToArray());
            var query = from sheet in sheetNames
                        join table in tableNames on sheet equals SpreadsheetUtility.Ellipsis(table)
                        where StringUtility.GlobMany(table, FilterProperties.FilterExpression)
                        orderby table
                        select sheet;
            var filterExpression = string.Join(";", query);
            var revision = dataBase.Dispatcher.Invoke(() => dataBase.DataBaseInfo.Revision);
            var dataSet = await dataBase.GetDataSetAsync(authentication, DataSetType.OmitContent, filterExpression, revision);
            this.ReadDataSet(dataSet, path);
            await dataBase.ImportAsync(authentication, dataSet, this.Message);
            this.Out.WriteLine($"importing data has been completed.");
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        [CommandProperty]
        public bool OmitAttribute
        {
            get;
            set;
        }

        [CommandProperty]
        public bool OmitSignatureDate
        {
            get;
            set;
        }

        [CommandPropertyRequired('m', AllowName = true, IsExplicit = true)]
        public string Message
        {
            get; set;
        }

        [CommandProperty('r', AllowName = true)]
        public string Revision
        {
            get; set;
        }

        [CommandProperty("database")]
        public string DataBaseName
        {
            get
            {
                if (this.dataBaseName != null)
                    return this.dataBaseName;
                if (this.CommandContext.Drive is DataBasesConsoleDrive drive)
                    return drive.DataBaseName;
                return null;
            }
            set => this.dataBaseName = value;
        }

        [CommandProperty]
        public bool SaveEach
        {
            get; set;
        }

        [CommandProperty("force")]
        public bool IsForce
        {
            get; set;
        }

        private int Comparison(object x, object y)
        {
            if (x is IDictionary)
                return 1;
            if (y is IDictionary)
                return -1;
            return $"{x}".CompareTo($"{y}");
        }

        private static string GetName()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                try
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        RedirectStandardOutput = true,
                        FileName = "sw_vers",
                        Arguments = "-productName",
                        UseShellExecute = false,
                    };
                    var process = System.Diagnostics.Process.Start(startInfo);
                    var text = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return "numbers";
                }
                catch
                {
                    return "calc";
                }
            }
            else
            {
                return "excel";
            }
        }

        private void ReadDataSet(CremaDataSet dataSet, string filename)
        {
            using var reader = new SpreadsheetReader(filename);
            reader.Read(dataSet);
        }

        private void WriteDataSet(CremaDataSet dataSet, string filename, SpreadsheetWriterSettings settings)
        {
            var path = Path.GetFullPath(Path.Combine(this.CommandContext.BaseDirectory, filename));
            using (var writer = new SpreadsheetWriter(dataSet, settings))
            {
                writer.Progress += Writer_Progress;
                writer.Write(path);
            }
            this.WriteFooter(path, settings);
        }

        private void WriteDataSetToDirectory(CremaDataSet dataSet, string path, SpreadsheetWriterSettings settings)
        {
            var directory = Path.GetFullPath(Path.Combine(this.CommandContext.BaseDirectory, path));
            if (settings.OmitType == false)
            {
                for (var i = 0; i < dataSet.Types.Count; i++)
                {
                    var item = dataSet.Types[i];
                    var filename = Path.Combine(directory, $"${item.Name}.xlsx");
                    using (var writer = new SpreadsheetWriter(item, settings))
                    {
                        writer.Write(filename);
                    }
                    this.Out.WriteLine($"write type {ConsoleProgress.GetProgressString(i + 1, dataSet.Types.Count)} : {item.Name}");
                }
            }
            if (settings.OmitTable == false)
            {
                for (var i = 0; i < dataSet.Tables.Count; i++)
                {
                    var item = dataSet.Tables[i];
                    var filename = Path.Combine(directory, $"{item.Name}.xlsx");
                    using (var writer = new SpreadsheetWriter(item, settings))
                    {
                        writer.Write(filename);
                    }
                    this.Out.WriteLine($"write type {ConsoleProgress.GetProgressString(i + 1, dataSet.Tables.Count)} : {item.Name}");
                }
            }
            this.WriteFooter(directory, settings);
        }

        private void WriteFooter(string path, SpreadsheetWriterSettings settings)
        {
            var props = new Dictionary<string, object>();
            foreach (var item in settings.Properties.Keys)
            {
                props.Add($"{item}", settings.Properties[item]);
            }
            props.Add("Path", path);
            this.Out.WriteLine();
            this.CommandContext.WriteObject(props, TextSerializerType.Yaml);
        }

        private void Writer_Progress(object sender, ProgressEventArgs e)
        {
            if (e.Target is CremaDataType dataType)
            {
                this.Out.WriteLine($"write type {ConsoleProgress.GetProgressString(e.Index + 1, e.Count)} : {dataType.Name}");
            }
            else if (e.Target is CremaDataTable dataTable)
            {
                this.Out.WriteLine($"write table {ConsoleProgress.GetProgressString(e.Index + 1, e.Count)} : {dataTable.Name}");
            }
            else if (e.Target is IDictionary)
            {
                this.Out.WriteLine($"write header {ConsoleProgress.GetProgressString(e.Index + 1, e.Count)}");
            }
        }

        private void ValidateExport(string filename)
        {
            var path = PathUtility.GetFullPath(filename, this.CommandContext.BaseDirectory);
            if (this.SaveEach == false && File.Exists(path) == true && this.IsForce == false)
                throw new InvalidOperationException("해당 파일이 이미 존재합니다.");
            if (this.SaveEach == true && Directory.Exists(path) == true && this.IsForce == false)
                throw new InvalidOperationException("해당 폴더가 이미 존재합니다.");
        }

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}