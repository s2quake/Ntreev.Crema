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

using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.RuntimeService;
using JSSoft.Library.Commands;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands
{
    [Export(typeof(ICommand))]
    [CommandStaticProperty(typeof(FilterSettings))]
    [CommandStaticProperty(typeof(DataBaseSettings))]
    class GetDataCommand : CommandAsyncBase
    {
        [Import]
        private IRuntimeService service = null;
        [ImportMany]
        private IEnumerable<IDataSerializer> serializers = null;
        [Import]
        private Lazy<CommandContext> commandContext = null;

        public GetDataCommand()
            : base("get-data")
        {

        }

        [CommandPropertyRequired]
        public string Address
        {
            get; set;
        }

        [CommandPropertyRequired]
        public string Filename
        {
            get; set;
        }

        [CommandPropertyRequired]
        [DefaultValue("bin")]
        public string OutputType
        {
            get; set;
        }

        [CommandProperty]
        [Description("개발 전용으로 생성합니다.")]
        [Obsolete]
        public bool Devmode
        {
            get; set;
        }

        [CommandProperty]
        public string Revision
        {
            get; set;
        }

        [CommandProperty]
#if DEBUG
        [DefaultValue("en-US")]
#else
        [DefaultValue("")]
#endif
        public string Culture
        {
            get; set;
        }

        protected override async Task OnExecuteAsync()
        {
            if (this.Culture != string.Empty)
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(this.Culture);
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(this.Culture);
            }

            this.Out.WriteLine("receiving info");
            var metaData = await service.GetDataGenerationDataAsync(this.Address, DataBaseSettings.DataBaseName, DataBaseSettings.Tags, FilterSettings.FilterExpression, this.Revision);

            this.Out.WriteLine("data serializing.");
            var serializer = this.serializers.FirstOrDefault(item => item.Name == this.OutputType);
            serializer.Serialize(this.Filename, metaData);
            this.Out.WriteLine("data serialized.");
        }
    }
}
