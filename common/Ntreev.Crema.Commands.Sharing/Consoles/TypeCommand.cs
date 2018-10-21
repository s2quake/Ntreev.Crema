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

using Ntreev.Crema.Commands.Consoles.Properties;
using Ntreev.Crema.Commands.Consoles.Serializations;
using Ntreev.Crema.Commands.Consoles.TypeTemplate;
using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using Ntreev.Library;
using Ntreev.Library.Commands;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources", IsShared = true)]
    class TypeCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public TypeCommand(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
        }

        [ConsoleModeOnly]
        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath))]
        public async Task CreateAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var category = await this.GetCategoryAsync(this.CategoryPath ?? this.GetCurrentDirectory());
            var typeNames = this.GetTypeNames();
            var template = await category.NewTypeAsync(authentication);
            var typeName = NameUtility.GenerateNewName("Type", typeNames);
            var typeInfo = JsonTypeInfo.Default;
            typeInfo.TypeName = typeName;

            try
            {
                if (JsonEditorHost.TryEdit(ref typeInfo) == false)
                    return;
                if (this.CommandContext.ReadYesOrNo($"do you want to create type '{typeInfo.TypeName}'?") == false)
                    return;

                await template.SetTypeNameAsync(authentication, typeInfo.TypeName);
                await template.SetIsFlagAsync(authentication, typeInfo.IsFlag);
                await template.SetCommentAsync(authentication, typeInfo.Comment);
                foreach (var item in typeInfo.Members)
                {
                    var member = await template.AddNewAsync(authentication);
                    await member.SetNameAsync(authentication, item.Name);
                    await member.SetValueAsync(authentication, item.Value);
                    await member.SetCommentAsync(authentication, item.Comment);
                    await template.EndNewAsync(authentication, member);
                }
                await template.EndEditAsync(authentication);
                template = null;
            }
            finally
            {
                if (template != null)
                {
                    await template.CancelEditAsync(authentication);
                }
            }
        }

        [ConsoleModeOnly]
        [CommandMethod]
        public async Task EditAsync([CommandCompletion(nameof(GetTypeNames))]string typeName)
        {
            var type = await this.GetTypeAsync(typeName);
            var template = type.Dispatcher.Invoke(() => type.Template);
            var domain = template.Dispatcher.Invoke(() => template.Domain);
            var authentication = this.CommandContext.GetAuthentication(this);
            var contains = domain == null ? false : await domain.Users.ContainsAsync(authentication.ID);

            if (contains == false)
                await template.BeginEditAsync(authentication);

            try
            {
                if (await TemplateEditor.EditAsync(template, authentication) == true)
                {
                    template = null;
                }
            }
            finally
            {
                if (template != null)
                {
                    await template.CancelEditAsync(authentication);
                }
            }
        }

        [CommandMethod]
        public async Task RenameAsync([CommandCompletion(nameof(GetTypeNames))]string typeName, string newTypeName)
        {
            var type = await this.GetTypeAsync(typeName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await type.RenameAsync(authentication, newTypeName);
        }

        [CommandMethod]
        public async Task MoveAsync([CommandCompletion(nameof(GetTypeNames))]string typeName, [CommandCompletion(nameof(GetCategoryPaths))]string categoryPath)
        {
            var type = await this.GetTypeAsync(typeName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await type.MoveAsync(authentication, categoryPath);
        }

        [CommandMethod]
        public async Task DeleteAsync([CommandCompletion(nameof(GetTypeNames))]string typeName)
        {
            var type = await this.GetTypeAsync(typeName);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.CommandContext.ConfirmToDelete() == true)
            {
                await type.DeleteAsync(authentication);
            }
        }

        [CommandMethod]
        public async Task SetTagsAsync([CommandCompletion(nameof(GetTypeNames))]string typeName, string tags)
        {
            var type = await this.GetTypeAsync(typeName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var template = type.Template;
            await template.BeginEditAsync(authentication);
            try
            {
                await template.SetTagsAsync(authentication, (TagInfo)tags);
                await template.EndEditAsync(authentication);
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                throw;
            }
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath))]
        public async Task CopyAsync([CommandCompletion(nameof(GetTypeNames))]string typeName, string newTypeName)
        {
            var type = await this.GetTypeAsync(typeName);
            var categoryPath = this.CategoryPath ?? this.GetCurrentDirectory();
            var authentication = this.CommandContext.GetAuthentication(this);
            await type.CopyAsync(authentication, newTypeName, categoryPath);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task ViewAsync([CommandCompletion(nameof(GetPaths))]string typeItemName, string revision = null)
        {
            var typeItem = await this.GetTypeItemAsync(typeItemName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataSet = await typeItem.GetDataSetAsync(authentication, revision);
            var props = dataSet.ToDictionary(false, true);
            this.CommandContext.WriteObject(props, FormatProperties.Format);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task LogAsync([CommandCompletion(nameof(GetPaths))]string typeItemName, string revision = null)
        {
            var typeItem = await this.GetTypeItemAsync(typeItemName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var logs = await typeItem.GetLogAsync(authentication, revision);

            foreach (var item in logs)
            {
                this.CommandContext.WriteObject(item.ToDictionary(), FormatProperties.Format);
                this.CommandContext.WriteLine();
            }
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(TagsProperties))]
        public void List()
        {
            var typeNames = this.GetTypeNames((TagInfo)TagsProperties.Tags, FilterProperties.FilterExpression);
            this.CommandContext.WriteList(typeNames);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync([CommandCompletion(nameof(GetTypeNames))]string typeName)
        {
            var type = await this.GetTypeAsync(typeName);
            var typeInfo = type.Dispatcher.Invoke(() => type.TypeInfo);
            this.CommandContext.WriteObject(typeInfo.ToDictionary(), FormatProperties.Format);
        }

        [CommandProperty]
        [CommandCompletion(nameof(GetCategoryPaths))]
        public string CategoryPath
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is DataBasesConsoleDrive drive && drive.DataBaseName != string.Empty;

        private async Task<IType> GetTypeAsync(string typeName)
        {
            var dataBase = await this.DataBases.Dispatcher.InvokeAsync(() => this.DataBases[this.Drive.DataBaseName]);
            var type = await dataBase.Dispatcher.InvokeAsync(() =>
            {
                if (NameValidator.VerifyItemPath(typeName) == true)
                    return dataBase.TypeContext[typeName] as IType;
                return dataBase.TypeContext.Types[typeName];
            });
            if (type == null)
                throw new TypeNotFoundException(typeName);
            return type;
        }

        private async Task<ITypeCategory> GetCategoryAsync(string categoryPath)
        {
            var dataBase = await this.DataBases.Dispatcher.InvokeAsync(() => this.DataBases[this.Drive.DataBaseName]);
            var category = await dataBase.Dispatcher.InvokeAsync(() => dataBase.TypeContext.Categories[categoryPath]);
            if (category == null)
                throw new CategoryNotFoundException(categoryPath);
            return category;
        }

        private async Task<ITypeItem> GetTypeItemAsync([CommandCompletion(nameof(GetPaths))]string typeItemName)
        {
            var dataBase = await this.DataBases.Dispatcher.InvokeAsync(() => this.DataBases[this.Drive.DataBaseName]);
            var typeItem = await dataBase.Dispatcher.InvokeAsync(() =>
            {
                if (NameValidator.VerifyItemPath(typeItemName) == true || NameValidator.VerifyCategoryPath(typeItemName) == true)
                    return dataBase.TypeContext[typeItemName];
                return dataBase.TypeContext.Types[typeItemName] as ITypeItem;
            });
            if (typeItem == null)
                throw new TypeNotFoundException(typeItemName);
            return typeItem;
        }

        private string[] GetTypeNames()
        {
            return GetTypeNames(TagInfo.All, null);
        }

        private string[] GetTypeNames(TagInfo tags, string filterExpress)
        {
            var dataBase = this.DataBases.Dispatcher.Invoke(() => this.DataBases[this.Drive.DataBaseName]);
            return dataBase.Dispatcher.Invoke(() =>
            {
                var query = from item in dataBase.TypeContext.Types
                            where StringUtility.GlobMany(item.Name, filterExpress)
                            where (item.TypeInfo.DerivedTags & tags) == tags
                            orderby item.Name
                            select item.Name;

                return query.ToArray();
            });
        }

        private string[] GetCategoryPaths()
        {
            var dataBase = this.DataBases.Dispatcher.Invoke(() => this.DataBases[this.Drive.DataBaseName]);
            return dataBase.Dispatcher.Invoke(() =>
            {
                var query = from item in dataBase.TypeContext.Categories
                            orderby item.Path
                            select item.Path;
                return query.ToArray();
            });
        }

        private string[] GetPaths()
        {
            var dataBase = this.DataBases.Dispatcher.Invoke(() => this.DataBases[this.Drive.DataBaseName]);
            return dataBase.Dispatcher.Invoke(() =>
            {
                var query = from item in dataBase.TypeContext.Categories
                            orderby item.Path
                            select item;

                var itemList = new List<string>(dataBase.TypeContext.Count());
                foreach (var item in query)
                {
                    itemList.Add(item.Path);
                    itemList.AddRange(from type in item.Types orderby type.Name select type.Name);
                }
                return itemList.ToArray();
            });
        }

        private string GetCurrentDirectory()
        {
            if (this.CommandContext.Drive is DataBasesConsoleDrive root)
            {
                var dataBasePath = new DataBasePath(this.CommandContext.Path);
                if (dataBasePath.ItemPath != string.Empty)
                    return dataBasePath.ItemPath;
            }
            return PathUtility.Separator;
        }

        private DataBasesConsoleDrive Drive => this.CommandContext.Drive as DataBasesConsoleDrive;

        private ICremaHost CremaHost => this.cremaHost;

        private IDataBaseCollection DataBases => this.cremaHost.GetService(typeof(IDataBaseCollection)) as IDataBaseCollection;
    }
}
