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

#pragma warning disable 0612

using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace JSSoft.Crema.Presentation.Home.Services.ViewModels
{
    class ConnectionItemCollection : ObservableCollection<ConnectionItemViewModel>
    {
        private const string versionString = "5.0";
        private static readonly XmlSerializer serializer;
        private readonly CremaAppHostViewModel cremaAppHost;
        private readonly string filename;

        static ConnectionItemCollection()
        {
            serializer = XmlSerializer.FromTypes(new Type[] { typeof(ConnectionItemInfo[]), }).First();
        }

        private ConnectionItemCollection(CremaAppHostViewModel cremaAppHost, string filename)
            : this(cremaAppHost, filename, Enumerable.Empty<ConnectionItemInfo>())
        {

        }

        private ConnectionItemCollection(CremaAppHostViewModel cremaAppHost, string filename, IEnumerable<ConnectionItemInfo> items)
        {
            this.cremaAppHost = cremaAppHost;
            this.filename = filename;

            foreach (var item in items)
            {
                var version = item.Version;
                var itemViewModel = new ConnectionItemViewModel(this.cremaAppHost)
                {
                    Name = item.Name,
                    Address = item.Address,
                    DataBaseName = item.DataBaseName,
                    ID = item.ID,
                    ThemeColor = item.ThemeColor,
                    Theme = item.Theme ?? CremaAppHostViewModel.Themes.First().Key,
                    IsDefault = item.IsDefault,
                };

                if (item.Password != null)
                    itemViewModel.Password = version == null ? StringUtility.Encrypt(item.Password, item.ID) : item.Password;
                base.Add(itemViewModel);
            }
            this.CollectionChanged += ConnectionItemInfoCollection_CollectionChanged;
        }

        public new void Add(ConnectionItemViewModel viewModel)
        {
            foreach (var item in this)
            {
                if (item.Equals(viewModel) == true)
                    return;
            }
            base.Add(viewModel);
        }

        public void Write()
        {
            DirectoryUtility.PrepareFile(this.filename);

            var connectionItemInfoList = new List<ConnectionItemInfo>(this.Count);

            foreach (var item in this)
            {
                if (item.IsTemporary == true)
                    continue;
                var connectionItemInfo = new ConnectionItemInfo()
                {
                    Version = versionString,
                    Name = item.Name,
                    Address = item.Address,
                    DataBaseName = item.DataBaseName,
                    ID = item.ID,
                    Password = item.Password,
                    ThemeColor = item.ThemeColor,
                    Theme = item.Theme,
                    IsDefault = item.IsDefault,
                };
                connectionItemInfoList.Add(connectionItemInfo);
            }

            FileUtility.Backup(this.filename);
            using var stream = new FileStream(this.filename, FileMode.Create);
            serializer.Serialize(stream, connectionItemInfoList.ToArray());
        }

        public static ConnectionItemCollection Read(CremaAppHostViewModel cremaAppHost, string filename)
        {
            try
            {
                using var stream = new FileStream(filename, FileMode.Open);
                var items = serializer.Deserialize(stream) as ConnectionItemInfo[];
                var collection = new ConnectionItemCollection(cremaAppHost, filename, items);
                return collection;
            }
            catch
            {
                return new ConnectionItemCollection(cremaAppHost, filename);
            }
        }

        private void ConnectionItemInfoCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Write();
        }
    }
}
