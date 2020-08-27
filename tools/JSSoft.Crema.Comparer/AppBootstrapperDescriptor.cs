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

using Caliburn.Micro;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace JSSoft.Crema.Comparer
{
    public class AppBootstrapperDescriptor : AppBootstrapperDescriptorBase
    {
        private CompositionContainer container;

        public override Type ModelType => typeof(IShell);

        protected override void OnInitialize(IEnumerable<Assembly> assemblies, IEnumerable<Tuple<Type, object>> parts)
        {
            var catalog = this.CreateCatalog(assemblies);
            var container = new CompositionContainer(catalog);
            var batch = this.CreateBatch(parts);
            container.Compose(batch);
            this.container = container;
        }

        protected override object GetInstance(Type service, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(service) : key;
            var exports = this.container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
                return exports.First();

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<Assembly> GetAssemblies()
        {
            var assemblyList = new List<Assembly>(base.GetAssemblies())
            {
                Assembly.GetExecutingAssembly()
            };

            var assembliesByName = new Dictionary<string, Assembly>();
            foreach (var item in assemblyList)
            {
                assembliesByName.Add(item.FullName, item);
            }

            if (Execute.InDesignMode == false)
            {
                var query = from directory in EnumerableUtility.Friends(AppDomain.CurrentDomain.BaseDirectory, this.SelectPath())
                            let catalog = new DirectoryCatalog(directory)
                            from file in catalog.LoadedFiles
                            select file;

                foreach (var item in query.Distinct())
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(item);
                        if (assembliesByName.ContainsKey(assembly.FullName) == false)
                        {
                            assembliesByName.Add(assembly.FullName, assembly);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return assembliesByName.Values.ToArray();
        }

        protected override IEnumerable<object> GetInstances(Type service)
        {
            return this.container.GetExportedValues<object>(AttributedModelServices.GetContractName(service));
        }

        protected override void OnBuildUp(object instance)
        {
            this.container.SatisfyImportsOnce(instance);
        }

        protected override void OnDispose()
        {
            this.container.Dispose();
        }

        protected virtual IEnumerable<string> SelectPath()
        {
            yield break;
        }

        private ComposablePartCatalog CreateCatalog(IEnumerable<Assembly> assemblies)
        {
            var catalog = new AggregateCatalog();
            foreach (var item in assemblies)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(item));
            }
            return catalog;
        }

        private CompositionBatch CreateBatch(IEnumerable<Tuple<Type, object>> parts)
        {
            var batch = new CompositionBatch();
            foreach (var item in parts)
            {
                var contractName = AttributedModelServices.GetContractName(item.Item1);
                var typeIdentity = AttributedModelServices.GetTypeIdentity(item.Item1);
                batch.AddExport(new Export(contractName, new Dictionary<string, object>
                {
                    {
                        "ExportTypeIdentity",
                        typeIdentity
                    }
                }, () => item.Item2));
            }
            batch.AddExportedValue<ICompositionService>(container);
            return batch;
        }
    }
}
