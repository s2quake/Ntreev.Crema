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

using Ntreev.Crema.Commands.Consoles.Serializations;
using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Commands.Consoles.TableTemplate
{
    static class TemplateEditor
    {
        public static async Task<bool> EditColumnsAsync(ITableTemplate template, Authentication authentication)
        {
            var columnCount = template.Dispatcher.Invoke(() => template.Count);
            var dataTypes = template.Dispatcher.Invoke(() => template.SelectableTypes);
            var columnList = new List<JsonColumnInfos.ItemInfo>(columnCount);
            var idToColumn = new Dictionary<Guid, ITableColumn>(columnCount);

            template.Dispatcher.Invoke(() =>
            {
                foreach (var item in template)
                {
                    var column = new JsonColumnInfos.ItemInfo()
                    {
                        ID = Guid.NewGuid(),
                        Name = item.Name,
                        IsKey = item.IsKey,
                        DataType = item.DataType,
                        Comment = item.Comment,
                        IsUnique = item.IsUnique,
                        AutoIncrement = item.AutoIncrement,
                        DefaultValue = item.DefaultValue,
                        Tags = (string)item.Tags,
                        IsReadOnly = item.IsReadOnly,
                        DisallowNull = !item.AllowNull,
                    };
                    idToColumn.Add(column.ID, item);
                    columnList.Add(column);
                }
            });

            var schema = JsonSchemaUtility.CreateSchema(typeof(JsonColumnInfos));
            var itemsSchema = schema.Properties[nameof(JsonColumnInfos.Items)];
            var itemSchema = itemsSchema.Items.First();
            var dataTypeSchema = itemSchema.Properties[nameof(JsonColumnInfos.ItemInfo.DataType)];
            dataTypeSchema.SetEnums(dataTypes);
            var tagSchema = itemSchema.Properties[nameof(JsonColumnInfos.ItemInfo.Tags)];
            tagSchema.SetEnums(TagInfoUtility.Names);

            var columns = new JsonColumnInfos() { Items = columnList.ToArray() };

            using (var editor = new JsonEditorHost(columns, schema))
            {
                if (editor.Execute() == false)
                    return false;

                columns = editor.Read<JsonColumnInfos>();
            }

            //template.Dispatcher.Invoke(() =>
            //{
            foreach (var item in idToColumn.Keys.ToArray())
            {
                if (columns.Items.Any(i => i.ID == item) == false)
                {
                    var column = idToColumn[item];
                    await column.DeleteAsync(authentication);
                    idToColumn.Remove(item);
                }
            }

            for (var i = 0; i < columns.Items.Length; i++)
            {
                var item = columns.Items[i];
                if (item.ID == Guid.Empty)
                {
                    var column = await template.AddNewAsync(authentication);
                    item = await InitializeFieldsAsync(authentication, item, column);
                    await template.EndNewAsync(authentication, column);
                    item.ID = Guid.NewGuid();
                    idToColumn.Add(item.ID, column);
                    columns.Items[i] = item;
                }
                else if (idToColumn.ContainsKey(item.ID) == true)
                {
                    var column = idToColumn[item.ID];
                    await SetFieldsAsync(authentication, item, column);
                }
                else
                {
                    throw new InvalidOperationException($"{item.ID} is not existed column.");
                }
            }

            for (var i = 0; i < columns.Items.Length; i++)
            {
                var item = columns.Items[i];
                var column = idToColumn[item.ID];
                await column.SetIndexAsync(authentication, i);
            }
            //});

            return true;
        }

        private static async Task<JsonColumnInfos.ItemInfo> InitializeFieldsAsync(Authentication authentication, JsonColumnInfos.ItemInfo item, ITableColumn column)
        {
            await column.SetNameAsync(authentication, item.Name);
            await column.SetDataTypeAsync(authentication, item.DataType);
            await column.SetCommentAsync(authentication, item.Comment);
            await column.SetTagsAsync(authentication, (TagInfo)item.Tags);
            await column.SetIsReadOnlyAsync(authentication, item.IsReadOnly);
            await column.SetIsUniqueAsync(authentication, item.IsUnique);
            await column.SetAutoIncrementAsync(authentication, item.AutoIncrement);
            await column.SetDefaultValueAsync(authentication, item.DefaultValue);
            await column.SetAllowNullAsync(authentication, !item.DisallowNull);
            return item;
        }

        private static async Task SetFieldsAsync(Authentication authentication, JsonColumnInfos.ItemInfo item, ITableColumn column)
        {
            if (column.Name != item.Name)
                await column.SetNameAsync(authentication, item.Name);
            if (column.DataType != item.DataType)
                await column.SetDataTypeAsync(authentication, item.DataType);
            if (column.Comment != item.Comment)
                await column.SetCommentAsync(authentication, item.Comment);
            if (column.Tags != (TagInfo)item.Tags)
                await column.SetTagsAsync(authentication, (TagInfo)item.Tags);
            if (column.IsReadOnly != item.IsReadOnly)
                await column.SetIsReadOnlyAsync(authentication, item.IsReadOnly);
            if (column.IsUnique != item.IsUnique)
                await column.SetIsUniqueAsync(authentication, item.IsUnique);
            if (column.AutoIncrement != item.AutoIncrement)
                await column.SetAutoIncrementAsync(authentication, item.AutoIncrement);
            if (column.DefaultValue != item.DefaultValue)
                await column.SetDefaultValueAsync(authentication, item.DefaultValue);
            if (column.AllowNull != !item.DisallowNull)
                await column.SetAllowNullAsync(authentication, !item.DisallowNull);
        }
    }
}
