﻿// Released under the MIT License.
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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JSSoft.Crema.Services
{
    public class CremaBootstrapper : IServiceProvider, IDisposable
    {
        private const string pluginsString = "plugins";
        private const string serializersString = "serializers";
        private const string repoModulesString = "repo-modules";
        private CompositionContainer container;
        private CremaSettings settings;

        public CremaBootstrapper()
        {
            this.Initialize();
        }

        public static void CreateRepository(IServiceProvider serviceProvider, string basePath, string repositoryModule, string fileType)
        {
            CreateRepository(serviceProvider, basePath, repositoryModule, fileType, string.Empty);
        }

        public static void CreateRepository(IServiceProvider serviceProvider, string basePath, string repositoryModule, string fileType, string dataBaseUrl)
        {
            CreateRepositoryInternal(serviceProvider, basePath, repositoryModule, fileType, dataBaseUrl, UserContext.GenerateDefaultUserInfos(), new CremaDataSet());
        }

        public static void ValidateRepository(IServiceProvider serviceProvider, string basePath, params string[] dataBaseNames)
        {
            var repositoryModule = GetRepositoryModule(basePath);
            var fileType = GetFileType(basePath);
            var repositoryProvider = GetRepositoryProvider(serviceProvider, repositoryModule);
            var serializer = GetSerializer(serviceProvider, fileType);
            var validationPath = Path.Combine(basePath, "validation");
            var logService = new LogServiceHost("validation", validationPath, true) { Verbose = LogLevel.Info, };

            var repositoryPath = CremaHost.GetPath(basePath, CremaPath.RepositoryDataBases);
            var items = repositoryProvider.GetRepositories(repositoryPath);

            if (dataBaseNames.Length > 0)
                items = items.Intersect(dataBaseNames).ToArray();

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var tempPath = Path.Combine(validationPath, item);
                var repositorySettings = new RepositorySettings()
                {
                    RemotePath = repositoryPath,
                    RepositoryName = item,
                    BasePath = tempPath,
                    LogService = logService,
                };
                var repository = repositoryProvider.CreateInstance(repositorySettings);
                try
                {
                    serializer.Validate(repository.BasePath, typeof(CremaDataSet), ObjectSerializerSettings.Empty);
                    repository.Dispose();
                    DirectoryUtility.Delete(tempPath);
                    logService.Info($"[{i + 1}/{items.Length}]{item}: OK");
                }
                catch (Exception e)
                {
                    logService.Error(e);
                    logService.Info($"[{i + 1}/{items.Length}]{item}: Fail");
                }
            }
        }

        public static void UpgradeRepository(IServiceProvider serviceProvider, string basePath, params string[] dataBaseNames)
        {
            var repositoryModule = GetRepositoryModule(basePath);
            var fileType = GetFileType(basePath);
            var repositoryProvider = GetRepositoryProvider(serviceProvider, repositoryModule);
            var serializer = GetSerializer(serviceProvider, fileType);
            var upgradePath = Path.Combine(basePath, "upgrade");
            var logService = new LogServiceHost("upgrade", upgradePath, true) { Verbose = LogLevel.Info, };

            var repositoryPath = CremaHost.GetPath(basePath, CremaPath.RepositoryDataBases);
            var items = repositoryProvider.GetRepositories(repositoryPath);

            if (dataBaseNames.Length > 0)
                items = items.Intersect(dataBaseNames).ToArray();

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var tempPath = Path.Combine(upgradePath, item);
                var repositorySettings = new RepositorySettings()
                {
                    RemotePath = repositoryPath,
                    RepositoryName = item,
                    BasePath = tempPath,
                    LogService = logService,
                };
                try
                {
                    if (UpgradeRepository(repositoryProvider, serializer, repositorySettings) == true)
                    {
                        logService.Info($"[{i + 1}/{items.Length}]{item}: upgraded");
                    }
                    else
                    {
                        logService.Info($"[{i + 1}/{items.Length}]{item}: skip");
                    }
                    DirectoryUtility.Delete(tempPath);
                }
                catch (Exception e)
                {
                    logService.Error(e);
                    logService.Info($"[{i + 1}/{items.Length}]{item}: fail");
                }
            }
        }

        public static void MigrateRepository(IServiceProvider serviceProvider, string basePath, string migrationModule, string repositoryUrl, bool force)
        {
            RepositoryMigrator.Migrate(serviceProvider, basePath, migrationModule, repositoryUrl, force);
        }

        public static void CleanRepository(IServiceProvider serviceProvider, string basePath)
        {
            DirectoryUtility.Delete(CremaHost.GetPath(basePath, CremaPath.Caches));
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            if (typeof(IEnumerable).IsAssignableFrom(serviceType) && serviceType.GenericTypeArguments.Length == 1)
            {
                var itemType = serviceType.GenericTypeArguments.First();
                var items = this.GetInstances(itemType);
                var listGenericType = typeof(List<>);
                var list = listGenericType.MakeGenericType(itemType);
                var ci = list.GetConstructor(new System.Type[] { typeof(int) });
                var instance = ci.Invoke(new object[] { items.Count() }) as IList;
                foreach (var item in items)
                {
                    instance.Add(item);
                }
                return instance;
            }
            else
            {
                return this.GetInstance(serviceType);
            }
        }

        public void Dispose()
        {
            this.container?.Dispose();
            this.container = null;
            this.OnDisposed(EventArgs.Empty);
        }

        public virtual IEnumerable<Tuple<System.Type, object>> GetParts()
        {
            yield return new Tuple<System.Type, object>(typeof(CremaBootstrapper), this);
            yield return new Tuple<System.Type, object>(typeof(IServiceProvider), this);
        }

        public virtual IEnumerable<Assembly> GetAssemblies()
        {
            var assemblyList = new List<Assembly>();

            if (Assembly.GetEntryAssembly() != null)
            {
                assemblyList.Add(Assembly.GetEntryAssembly());
            }

            var query = from directory in EnumerableUtility.Friends(AppDomain.CurrentDomain.BaseDirectory, this.SelectPath())
                        let catalog = new DirectoryCatalog(directory)
                        from file in catalog.LoadedFiles
                        select file;

            foreach (var item in query)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(item);
                    assemblyList.Add(assembly);
                    CremaLog.Debug(assembly.Location);
                }
                catch
                {

                }
            }

            return assemblyList.Distinct();
        }

        public virtual IEnumerable<string> SelectPath()
        {
            var items = SelectPath(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var item in items)
            {
                yield return item;
            }
        }

        public static IEnumerable<string> SelectPath(string basePath)
        {
            var dllPath = basePath;
            var rootPath = Path.GetDirectoryName(dllPath);
            var repositoryPath = Path.Combine(rootPath, RepositoryModulesPath);
            if (Directory.Exists(repositoryPath) == true)
            {
                foreach (var item in Directory.GetDirectories(repositoryPath))
                {
                    yield return item;
                }
            }

            var pluginsPath = Path.Combine(rootPath, pluginsString);
            if (Directory.Exists(pluginsPath) == true)
            {
                foreach (var item in Directory.GetDirectories(pluginsPath))
                {
                    yield return item;
                }
            }

            var serializersPath = Path.Combine(rootPath, serializersString);
            if (Directory.Exists(serializersPath) == true)
            {
                foreach (var item in Directory.GetDirectories(serializersPath))
                {
                    yield return item;
                }
            }
        }

        public string BasePath
        {
            get => this.settings.BasePath;
            set
            {
                var fullpath = PathUtility.GetFullPath(value);
                this.settings.BasePath = PathUtility.GetCaseSensitivePath(fullpath);
            }
        }

        public LogLevel Verbose
        {
            get => this.settings.Verbose;
            set => this.settings.Verbose = value;
        }

        public bool NoCache
        {
            get => this.settings.NoCache;
            set => this.settings.NoCache = value;
        }

        public string Culture
        {
            get => $"{System.Globalization.CultureInfo.CurrentCulture}";
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value != string.Empty)
                {
                    System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(value);
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(value);
                }
            }
        }

        public string[] DataBaseList
        {
            get => this.settings.DataBases;
            set => this.settings.DataBases = value;
        }

#if DEBUG
        public bool ValidationMode
        {
            get => this.settings.ValidationMode;
            set => this.settings.ValidationMode = value;
        }
#endif

        public static string RepositoryModulesPath => repoModulesString;

        public event EventHandler Disposed;

        protected virtual object GetInstance(System.Type type)
        {
            var contractName = AttributedModelServices.GetContractName(type);
            return this.container.GetExportedValue<object>(contractName);
        }

        protected virtual IEnumerable<object> GetInstances(System.Type type)
        {
            var contractName = AttributedModelServices.GetContractName(type);
            return this.container.GetExportedValues<object>(contractName);
        }

        protected virtual void OnInitialize()
        {
            var catalog = new AggregateCatalog();

            foreach (var item in this.GetAssemblies())
            {
                catalog.Catalogs.Add(new AssemblyCatalog(item));
            }

            this.container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();
            foreach (var item in this.GetParts())
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

            this.container.Compose(batch);
            this.settings = this.container.GetExportedValue<CremaSettings>();
        }

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        internal static IObjectSerializer GetSerializer(IServiceProvider serviceProvider, string fileType)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (fileType is null)
                throw new ArgumentNullException(nameof(fileType));

            var name = fileType == string.Empty ? "xml" : fileType;
            var serializers = serviceProvider.GetService(typeof(IEnumerable<IObjectSerializer>)) as IEnumerable<IObjectSerializer>;
            var serializer = serializers.FirstOrDefault(item => item.Name == name);
            if (serializer == null)
                throw new InvalidOperationException("no serializer");
            return serializer;
        }

        internal static IRepositoryProvider GetRepositoryProvider(IServiceProvider serviceProvider, string repositoryModule)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (repositoryModule is null)
                throw new ArgumentNullException(nameof(repositoryModule));

            var name = repositoryModule == string.Empty ? "git" : repositoryModule;
            var repositoryProviders = serviceProvider.GetService(typeof(IEnumerable<IRepositoryProvider>)) as IEnumerable<IRepositoryProvider>;
            var repositoryProvider = repositoryProviders.FirstOrDefault(item => item.Name == name);
            if (repositoryProvider == null)
                throw new InvalidOperationException(Resources.Exception_NoRepositoryModule);
            return repositoryProvider;
        }

        internal static IRepositoryMigrator GetRepositoryMigrator(IServiceProvider serviceProvider, string migrationModule)
        {
            var repositoryMigrators = serviceProvider.GetService(typeof(IEnumerable<IRepositoryMigrator>)) as IEnumerable<IRepositoryMigrator>;
            var repositoryMigrator = repositoryMigrators.FirstOrDefault(item => item.Name == migrationModule);
            if (repositoryMigrator == null)
                throw new InvalidOperationException("no repository migrator");
            return repositoryMigrator;
        }

        internal static void CreateRepositoryInternal(IServiceProvider serviceProvider, string basePath, string repositoryModule, string fileType, string dataBaseUrl
            , UserContextSerializationInfo userContextSerializationInfo
            , CremaDataSet dataSet)
        {
            ValidateCreateRepository(serviceProvider, basePath, repositoryModule, fileType, dataBaseUrl);
            var repositoryProvider = GetRepositoryProvider(serviceProvider, repositoryModule);
            var serializer = GetSerializer(serviceProvider, fileType);

            var tempPath = PathUtility.GetTempPath(true);
            var repositoryPath = Path.Combine(PathUtility.GetFullPath(basePath), CremaString.Repository);

            DirectoryUtility.Backup(repositoryPath);
            try
            {
                var usersRepo = DirectoryUtility.Prepare(repositoryPath, CremaString.Users);
                var usersPath = DirectoryUtility.Prepare(tempPath, CremaString.Users);
                var dataBasesRepo = DirectoryUtility.Prepare(repositoryPath, CremaString.DataBases);

                userContextSerializationInfo.WriteToDirectory(usersPath, serializer);
                repositoryProvider.InitializeRepository(usersRepo, usersPath, new LogPropertyInfo() { Key = LogPropertyInfo.VersionKey, Value = AppUtility.ProductVersion });

                if (dataBaseUrl == string.Empty)
                {
                    var dataBasesPath = DirectoryUtility.Prepare(tempPath, CremaString.DataBases);
                    dataSet.WriteToDirectory(dataBasesPath);
                    FileUtility.WriteAllText($"{CremaSchema.MajorVersion}.{CremaSchema.MinorVersion}", dataBasesPath, ".version");
                    repositoryProvider.InitializeRepository(dataBasesRepo, dataBasesPath);
                }
                else
                {
                    repositoryProvider.InitializeRepository(dataBasesRepo, dataBaseUrl);
                }

                var repoModulePath = FileUtility.WriteAllText(repositoryProvider.Name, repositoryPath, CremaString.Repo);
                var fileTypePath = FileUtility.WriteAllText(serializer.Name, repositoryPath, CremaString.File);

                FileUtility.WriteAllText(Resources.Text_README, basePath, "README.md");

                FileUtility.SetReadOnly(repoModulePath, true);
                FileUtility.SetReadOnly(fileTypePath, true);
                DirectoryUtility.SetVisible(repositoryPath, false);
                DirectoryUtility.Clean(repositoryPath);
            }
            catch
            {
                DirectoryUtility.Restore(repositoryPath);
                throw;
            }
        }

        private static string GetRepositoryModule(string basePath)
        {
            return FileUtility.ReadAllText(basePath, CremaString.Repository, CremaString.Repo).Trim();
        }

        private static string GetFileType(string basePath)
        {
            return FileUtility.ReadAllText(basePath, CremaString.Repository, CremaString.File).Trim();
        }

        private void Initialize()
        {
            CremaLog.Debug("Initialize.");
            this.OnInitialize();
            CremaLog.Debug("Initialized.");
        }

        private static void ValidateCreateRepository(IServiceProvider serviceProvider, string basePath, string repositoryModule, string fileType, string dataBaseUrl)
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (basePath is null)
                throw new ArgumentNullException(nameof(basePath));
            if (repositoryModule is null)
                throw new ArgumentNullException(nameof(repositoryModule));
            if (fileType is null)
                throw new ArgumentNullException(nameof(fileType));
            if (dataBaseUrl is null)
                throw new ArgumentNullException(nameof(dataBaseUrl));

            var directoryInfo = new DirectoryInfo(basePath);
            if (directoryInfo.Exists == false)
            {
                DirectoryUtility.Create(directoryInfo.FullName);
            }
            else if (DirectoryUtility.IsEmpty(directoryInfo.FullName) == false)
            {
                throw new ArgumentException("Path is not an empty directory.", nameof(basePath));
            }
        }

        private static bool UpgradeRepository(IRepositoryProvider repositoryProvider, IObjectSerializer serializer, RepositorySettings settings)
        {
            var repository = repositoryProvider.CreateInstance(settings);
            var versionPath = Path.Combine(repository.BasePath, ".version");
            var version = GetVersion(versionPath);
            if (version.Major == CremaSchema.MajorVersion && version.Minor == CremaSchema.MinorVersion)
            {
                repository.Dispose();
                return false;
            }
            else
            {
                var dataSet = serializer.Deserialize(repository.BasePath, typeof(CremaDataSet), ObjectSerializerSettings.Empty) as CremaDataSet;
                var files = serializer.Serialize(repository.BasePath, dataSet, ObjectSerializerSettings.Empty);

                File.WriteAllText(versionPath, $"{CremaSchema.MajorVersion}.{CremaSchema.MinorVersion}");
                foreach (var item in repository.Status())
                {
                    if (item.Status == RepositoryItemStatus.Untracked)
                    {
                        repository.Add(item.Path);
                    }
                }
                repository.Commit(Authentication.SystemID, $"upgrade: {CremaSchema.MajorVersion}.{CremaSchema.MinorVersion}");
                repository.Dispose();
                return true;
            }
        }

        private static Version GetVersion(string versionPath)
        {
            if (File.Exists(versionPath) == false)
                return new Version();
            try
            {
                return new Version(File.ReadAllText(versionPath));
            }
            catch
            {
                return new Version();
            }
        }
    }
}
