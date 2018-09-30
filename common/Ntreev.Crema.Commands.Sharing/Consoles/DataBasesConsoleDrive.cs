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

using Ntreev.Crema.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Ntreev.Library.IO;
using Ntreev.Library;
using System.Text.RegularExpressions;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleDrive))]
    public sealed class DataBasesConsoleDrive : ConsoleDriveBase, IPartImportsSatisfiedNotification
    {
        private readonly ICremaHost cremaHost;
        private DataBasePath dataBasePath;

        [ImportingConstructor]
        internal DataBasesConsoleDrive(ICremaHost cremaHost)
            : base("databases")
        {
            this.cremaHost = cremaHost;
        }

        public override string[] GetPaths()
        {
            return this.CremaHost.Dispatcher.Invoke(GetPathsImpl);

            string[] GetPathsImpl()
            {
                var pathList = new List<string>
                {
                    PathUtility.Separator
                };
                foreach (var item in this.DataBases)
                {
                    var items = item.DataBaseInfo.Paths.Select(i => $"{PathUtility.SeparatorChar}{item.Name}{i}");
                    pathList.AddRange(items);
                }
                return pathList.ToArray();
            }
        }

        public string DataBaseName => this.dataBasePath?.DataBaseName ?? string.Empty;

        public string Context => this.dataBasePath?.Context ?? string.Empty;

        public string ItemPath => this.dataBasePath?.ItemPath ?? string.Empty;

        protected override async Task OnSetPathAsync(Authentication authentication, string path)
        {
            var dataBaseName = this.DataBaseName;
            var dataBasePath = new DataBasePath(path);

            if (dataBaseName != string.Empty && dataBasePath.DataBaseName != dataBaseName)
            {
                var dataBase = this.DataBases.Dispatcher.Invoke(() => this.DataBases[dataBaseName]);
                await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    dataBase.Unloaded -= DataBase_Unloaded;
                });
                if (dataBase.IsLoaded == true)
                {
                    await dataBase.LeaveAsync(authentication);
                }
            }

            if (dataBasePath.DataBaseName != string.Empty && dataBasePath.DataBaseName != dataBaseName)
            {
                var dataBase = this.DataBases.Dispatcher.Invoke(() => this.DataBases[dataBasePath.DataBaseName]);
                if (dataBase.IsLoaded == false)
                    await dataBase.LoadAsync(authentication);
                await dataBase.EnterAsync(authentication);
                await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    dataBase.Unloaded += DataBase_Unloaded;
                });
            }

            this.dataBasePath = dataBasePath;
        }

        protected override async Task OnCreateAsync(Authentication authentication, string path, string name)
        {
            var target = await this.GetObjectAsync(authentication, path);

            if (target is ITableCategory tableCategory)
            {
                var dataBase = tableCategory.GetService(typeof(IDataBase)) as IDataBase;
                using (await UsingDataBase.SetAsync(dataBase, authentication))
                {
                    await tableCategory.AddNewCategoryAsync(authentication, name);
                }
            }
            else if (target is ITypeCategory typeCategory)
            {
                var dataBase = typeCategory.GetService(typeof(IDataBase)) as IDataBase;
                using (await UsingDataBase.SetAsync(dataBase, authentication))
                {
                    await typeCategory.AddNewCategoryAsync(authentication, name);
                }
            }
            else if (path == PathUtility.Separator)
            {
                var comment = this.CommandContext.ReadString("comment:");
                await this.DataBases.AddNewDataBaseAsync(authentication, name, comment);
            }
            else
            {
                var dataBasePath = new DataBasePath(path);
                if (dataBasePath.Context == string.Empty)
                    throw new PermissionDeniedException();
                throw new CategoryNotFoundException(path);
            }
        }

        protected override async Task OnMoveAsync(Authentication authentication, string path, string newPath)
        {
            var sourceObject = await this.GetObjectAsync(authentication, path);

            if (sourceObject is IType sourceType)
            {
                await this.MoveTypeAsync(authentication, sourceType, newPath);
            }
            else if (sourceObject is ITypeCategory sourceTypeCategory)
            {
                await this.MoveTypeCategoryAsync(authentication, sourceTypeCategory, newPath);
            }
            else if (sourceObject is ITable sourceTable)
            {
                await this.MoveTableAsync(authentication, sourceTable, newPath);
            }
            else if (sourceObject is ITableCategory sourceTableCategory)
            {
                await this.MoveTableCategoryAsync(authentication, sourceTableCategory, newPath);
            }
            else if (sourceObject is IDataBase dataBase)
            {
                await this.MoveDataBaseAsync(authentication, dataBase, newPath);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        protected override async Task OnDeleteAsync(Authentication authentication, string path)
        {
            var target = await this.GetObjectAsync(authentication, path);
            if (target is IDataBase)
            {
                var dataBase = target as IDataBase;
             await    dataBase.DeleteAsync(authentication);
            }
            else if (target is ITableItem tableItem)
            {
                var dataBase = tableItem.GetService(typeof(IDataBase)) as IDataBase;
                using (await UsingDataBase.SetAsync(dataBase, authentication))
                {
                    await tableItem.DeleteAsync(authentication);
                }
            }
            else if (target is ITypeItem typeItem)
            {
                var dataBase = typeItem.GetService(typeof(IDataBase)) as IDataBase;
                using (await UsingDataBase.SetAsync(dataBase, authentication))
                {
                    await typeItem.DeleteAsync(authentication);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private async Task MoveTypeCategoryAsync(Authentication authentication, ITypeCategory sourceCategory, string newPath)
        {
            var destPath = new DataBasePath(newPath);
            var destObject = await this.GetObjectAsync(authentication, destPath.Path);
            var dataBase = sourceCategory.GetService(typeof(IDataBase)) as IDataBase;
            var types = sourceCategory.GetService(typeof(ITypeCollection)) as ITypeCollection;

            if (destPath.DataBaseName != dataBase.Name)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destPath.Context != CremaSchema.TypeDirectory)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destObject is IType)
                throw new InvalidOperationException($"cannot move to : {destPath}");

            using (await UsingDataBase.SetAsync(dataBase, authentication))
            {
                if (destObject is ITypeCategory destCategory)
                {
                    if (sourceCategory.Parent != destCategory)
                        await sourceCategory.MoveAsync(authentication, destCategory.Path);
                }
                else
                {
                    if (NameValidator.VerifyCategoryPath(destPath.ItemPath) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    var itemName = new ItemName(destPath.ItemPath);
                    var categories = sourceCategory.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
                    if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceCategory.Name != itemName.Name && await types.ContainsAsync(itemName.Name) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceCategory.Parent.Path != itemName.CategoryPath)
                        await sourceCategory.MoveAsync(authentication, itemName.CategoryPath);
                    if (sourceCategory.Name != itemName.Name)
                        await sourceCategory.RenameAsync(authentication, itemName.Name);
                }
            }
        }

        private async Task MoveTypeAsync(Authentication authentication, IType sourceType, string newPath)
        {
            var destPath = new DataBasePath(newPath);
            var destObject = await this.GetObjectAsync(authentication, destPath.Path);
            var dataBase = sourceType.GetService(typeof(IDataBase)) as IDataBase;
            var types = sourceType.GetService(typeof(ITypeCollection)) as ITypeCollection;

            if (destPath.DataBaseName != dataBase.Name)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destPath.Context != CremaSchema.TypeDirectory)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destObject is IType)
                throw new InvalidOperationException($"cannot move to : {destPath}");

            using (await UsingDataBase.SetAsync(dataBase, authentication))
            {
                if (destObject is ITypeCategory destCategory)
                {
                    if (sourceType.Category != destCategory)
                        await sourceType.MoveAsync(authentication, destCategory.Path);
                }
                else
                {
                    if (NameValidator.VerifyCategoryPath(destPath.ItemPath) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    var itemName = new ItemName(destPath.ItemPath);
                    var categories = sourceType.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
                    if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceType.Name != itemName.Name && await types.ContainsAsync(itemName.Name) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceType.Category.Path != itemName.CategoryPath)
                        await sourceType.MoveAsync(authentication, itemName.CategoryPath);
                    if (sourceType.Name != itemName.Name)
                        await sourceType.RenameAsync(authentication, itemName.Name);
                }
            }
        }

        private async Task MoveTableCategoryAsync(Authentication authentication, ITableCategory sourceCategory, string newPath)
        {
            var destPath = new DataBasePath(newPath);
            var destObject = await this.GetObjectAsync(authentication, destPath.Path);
            var dataBase = sourceCategory.GetService(typeof(IDataBase)) as IDataBase;
            var tables = sourceCategory.GetService(typeof(ITableCollection)) as ITableCollection;

            if (destPath.DataBaseName != dataBase.Name)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destPath.Context != CremaSchema.TableDirectory)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destObject is ITable)
                throw new InvalidOperationException($"cannot move to : {destPath}");

            using (await UsingDataBase.SetAsync(dataBase, authentication))
            {
                if (destObject is ITableCategory destCategory)
                {
                    if (sourceCategory.Parent != destCategory)
                        await sourceCategory.MoveAsync(authentication, destCategory.Path);
                }
                else
                {
                    if (NameValidator.VerifyCategoryPath(destPath.ItemPath) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    var itemName = new ItemName(destPath.ItemPath);
                    var categories = sourceCategory.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceCategory.Name != itemName.Name && await tables.ContainsAsync(itemName.Name) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceCategory.Parent.Path != itemName.CategoryPath)
                        await sourceCategory.MoveAsync(authentication, itemName.CategoryPath);
                    if (sourceCategory.Name != itemName.Name)
                        await sourceCategory.RenameAsync(authentication, itemName.Name);
                }
            }
        }

        private async Task MoveTableAsync(Authentication authentication, ITable sourceTable, string newPath)
        {
            var destPath = new DataBasePath(newPath);
            var destObject = await this.GetObjectAsync(authentication, destPath.Path);
            var dataBase = sourceTable.GetService(typeof(IDataBase)) as IDataBase;
            var tables = sourceTable.GetService(typeof(ITableCollection)) as ITableCollection;

            if (destPath.DataBaseName != dataBase.Name)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destPath.Context != CremaSchema.TableDirectory)
                throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destObject is ITable)
                throw new InvalidOperationException($"cannot move to : {destPath}");

            using (await UsingDataBase.SetAsync(dataBase, authentication))
            {
                if (destObject is ITableCategory destCategory)
                {
                    if (sourceTable.Category != destCategory)
                        await sourceTable.MoveAsync(authentication, destCategory.Path);
                }
                else
                {
                    if (NameValidator.VerifyCategoryPath(destPath.ItemPath) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    var itemName = new ItemName(destPath.ItemPath);
                    var categories = sourceTable.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceTable.Name != itemName.Name && await tables.ContainsAsync(itemName.Name) == true)
                        throw new InvalidOperationException($"cannot move to : {destPath}");
                    if (sourceTable.Category.Path != itemName.CategoryPath)
                        await sourceTable.MoveAsync(authentication, itemName.CategoryPath);
                    if (sourceTable.Name != itemName.Name)
                        await sourceTable.RenameAsync(authentication, itemName.Name);
                }
            }
        }

        private Task MoveDataBaseAsync(Authentication authentication, IDataBase dataBase, string newPath)
        {
            if (NameValidator.VerifyCategoryPath(newPath) == true)
                throw new InvalidOperationException($"cannot move {dataBase} to : {newPath}");
            var itemName = new ItemName(newPath);
            if (itemName.CategoryPath != PathUtility.Separator)
                throw new InvalidOperationException($"cannot move {dataBase} to : {newPath}");

            return dataBase.RenameAsync(authentication, itemName.Name);
        }

        public override async Task<object> GetObjectAsync(Authentication authentication, string path)
        {
            var dataBasePath = new DataBasePath(path);

            if (dataBasePath.DataBaseName == string.Empty)
                return null;

            var dataBase = this.DataBases[dataBasePath.DataBaseName];
            if (dataBase == null)
                throw new DataBaseNotFoundException(dataBasePath.DataBaseName);

            if (dataBasePath.Context == string.Empty)
                return dataBase;

            if (dataBasePath.ItemPath == string.Empty)
                return null;

            if (dataBase.IsLoaded == false)
                await dataBase.LoadAsync(authentication);

            if (dataBasePath.Context == CremaSchema.TableDirectory)
            {
                if (NameValidator.VerifyCategoryPath(dataBasePath.ItemPath) == true)
                    return dataBase.TableContext[dataBasePath.ItemPath];
                var item = dataBase.TableContext[dataBasePath.ItemPath + PathUtility.Separator];
                if (item != null)
                    return item;
                return dataBase.TableContext[dataBasePath.ItemPath];
            }
            else if (dataBasePath.Context == CremaSchema.TypeDirectory)
            {
                if (NameValidator.VerifyCategoryPath(dataBasePath.ItemPath) == true)
                    return dataBase.TypeContext[dataBasePath.ItemPath];
                var item = dataBase.TypeContext[dataBasePath.ItemPath + PathUtility.Separator];
                if (item != null)
                    return item;
                return dataBase.TypeContext[dataBasePath.ItemPath];
            }
            else
            {
                return null;
            }
        }

        private void DataBase_Unloaded(object sender, EventArgs e)
        {
            if (this.CommandContext.IsOnline == true)
                this.CommandContext.Path = PathUtility.Separator;
        }

        private ICremaHost CremaHost => this.cremaHost;

        private IDataBaseCollection DataBases => this.cremaHost.GetService(typeof(IDataBaseCollection)) as IDataBaseCollection;

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            this.CremaHost.Closed += (s, e) => this.dataBasePath = null;
        } 

        #endregion
    }
}
