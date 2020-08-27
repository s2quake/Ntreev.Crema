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

using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace JSSoft.Crema.Services
{
    public class UserConfiguration : ConfigurationBase, IUserConfiguration, IConfigurationCommitter
    {
        private readonly string schemaPath;
        private readonly string xmlPath;
        private readonly IConfigurationSerializer serializer = new ConfigurationSerializer();

        public UserConfiguration(string path, IEnumerable<IConfigurationPropertyProvider> propertiesProviders)
            : base(typeof(IUserConfiguration), propertiesProviders)
        {
            this.xmlPath = path;
            this.schemaPath = Path.ChangeExtension(path, ".xsd");
            try
            {
                if (File.Exists(this.xmlPath) == true)
                {
                    using var stream = File.OpenRead(this.xmlPath);
                    this.Read(stream, this.serializer);
                }
            }
            catch
            {

            }
        }

        public override string Name => "UserConfigs";

        public void Commit()
        {
            FileUtility.Prepare(this.xmlPath);
            using var stream = File.OpenWrite(this.xmlPath);
            this.Write(stream, this.serializer);
        }
    }
}