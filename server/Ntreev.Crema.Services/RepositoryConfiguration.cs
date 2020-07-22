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

using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ntreev.Crema.Services
{
    public class RepositoryConfiguration : ConfigurationBase, IRepositoryConfiguration, IConfigurationCommitter
    {
        private readonly ILogService logService;
        private readonly string itemName;
        private readonly IConfigurationSerializer serializer = new ConfigurationSerializer();

        public RepositoryConfiguration(ILogService logService, string itemName, IEnumerable<IConfigurationPropertyProvider> propertiesProvider)
            : base(typeof(IRepositoryConfiguration), propertiesProvider)
        {
            this.logService = logService;
            this.itemName = itemName;
            try
            {
                var filename = this.itemName + ".xml";
                using var stream = File.OpenRead(filename);
                this.Read(stream, this.serializer);
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
            }
        }

        public override string Name => "CremaConfigs";

        public void Commit()
        {
            try
            {
                throw new NotImplementedException();
                // this.WriteSchema(this.itemName + ".xsd");
                // this.Write(this.itemName + ".xml", Path.GetFileName(this.itemName) + ".xsd");
            }
            catch (Exception e)
            {
                this.logService.Debug(e);
                throw;
            }
        }
    }
}
