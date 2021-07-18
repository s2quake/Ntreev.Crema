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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    partial class DataBase
    {
        public async Task<Guid> ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (dataSet is null)
                    throw new ArgumentNullException(nameof(dataSet));
                if (comment is null)
                    throw new ArgumentNullException(nameof(comment));

                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ImportAsync), comment);
                    this.ValidateImport(authentication, dataSet, comment);
                    this.CremaHost.Sign(authentication);

                });
                var taskID = Guid.NewGuid();
                var filter = new CremaDataSetFilter()
                {
                    Tables = dataSet.Tables.Select(item => item.Name).ToArray(),
                    OmitContent = true
                };
                var targetSet = await this.GetDataSetAsync(authentication, filter, string.Empty);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.LockTypes(authentication, targetSet, comment);
                    this.LockTables(authentication, targetSet, comment);
                });

                try
                {
                    targetSet.SignatureDateProvider = new SignatureDateProvider(authentication.ID);
                    foreach (var item in targetSet.Tables)
                    {
                        var dataTable = dataSet.Tables[item.Name];
                        foreach (var row in dataTable.Rows)
                        {
                            item.ImportRow(row);
                        }
                        item.BeginLoad();
                        foreach (var row in item.Rows)
                        {
                            row.CreationInfo = authentication.SignatureDate;
                            row.ModificationInfo = authentication.SignatureDate;
                        }
                        item.ContentsInfo = authentication.SignatureDate;
                        item.EndLoad();
                    }
                    var dataBaseSet = await DataBaseSet.CreateAsync(this, targetSet);
                    await this.Repository.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            this.Repository.Modify(dataBaseSet);
                            this.Repository.Commit(authentication, comment);
                        }
                        catch
                        {
                            this.Repository.Revert();
                            throw;
                        }
                    });
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.UpdateTables(authentication, targetSet);
                    });
                    return taskID;
                }
                finally
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.UnlockTypes(authentication, targetSet);
                        this.UnlockTables(authentication, targetSet);
                    });
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        private void ValidateImport(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            if (comment == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed);

            if (this.IsLoaded == false)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);

            this.ValidateAccessType(authentication, AccessType.Editor);

            var tableList = new List<Table>(dataSet.Tables.Count);
            foreach (var item in dataSet.Tables)
            {
                var table = this.TableContext.Tables[item.Name];
                if (table == null)
                    throw new TableNotFoundException(item.Name);
                tableList.Add(table);
                table.ValidateAccessType(authentication, AccessType.Editor);
                table.ValidateHasNotBeingEditedType();
                table.ValidateIsNotBeingEdited();
            }

            var query = from item in tableList
                        where item.LockInfo.Path != item.Path
                        select item;

            foreach (var item in query)
            {
                item.ValidateLockInternal(authentication);
            }
        }

        private void LockTypes(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            Authentication.System.Sign(authentication.SignatureDate.DateTime);
            var query = from item in dataSet.Types
                        let type = this.TypeContext.Types[item.Name]
                        where type.LockInfo.Path != type.Path
                        select type;

            var items = query.ToArray();
            var comments = Enumerable.Repeat(comment, items.Length).ToArray();
            foreach (var item in items)
            {
                item.LockInternal(Authentication.System, comment);
                dataSet.Types[item.Name].ExtendedProperties[this] = true;
            }
            this.TypeContext.InvokeItemsLockedEvent(authentication, items, comments);
        }

        private void LockTables(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            Authentication.System.Sign(authentication.SignatureDate.DateTime);
            var query = from item in dataSet.Tables
                        let table = this.TableContext.Tables[item.Name]
                        where table.LockInfo.Path != table.Path
                        select table;

            var items = query.ToArray();
            var comments = Enumerable.Repeat(comment, items.Length).ToArray();
            foreach (var item in items)
            {
                item.LockInternal(Authentication.System, comment);
                dataSet.Tables[item.Name].ExtendedProperties[this] = true;
            }
            this.TableContext.InvokeItemsLockedEvent(Authentication.System, items, comments);
        }

        private void UnlockTypes(Authentication authentication, CremaDataSet dataSet)
        {
            Authentication.System.Sign(authentication.SignatureDate.DateTime);
            var query = from item in dataSet.Types
                        where item.ExtendedProperties.Contains(this)
                        select this.TypeContext.Types[item.Name];

            var items = query.ToArray();
            foreach (var item in items)
            {
                item.UnlockInternal(Authentication.System);
            }
            this.TypeContext.InvokeItemsUnlockedEvent(Authentication.System, items);
        }

        private void UnlockTables(Authentication authentication, CremaDataSet dataSet)
        {
            Authentication.System.Sign(authentication.SignatureDate.DateTime);
            var query = from item in dataSet.Tables
                        where item.ExtendedProperties.Contains(this)
                        select this.TableContext.Tables[item.Name];

            var items = query.ToArray();
            foreach (var item in items)
            {
                item.UnlockInternal(Authentication.System);
            }
            this.TableContext.InvokeItemsUnlockedEvent(Authentication.System, items);
        }

        private void UpdateTables(Authentication authentication, CremaDataSet dataSet)
        {
            var tableList = new List<Table>(dataSet.Tables.Count);
            foreach (var item in dataSet.Tables)
            {
                var table = this.TableContext.Tables[item.Name];
                tableList.Add(table);
                table.UpdateContent(item.TableInfo);
            }
            this.TableContext.InvokeItemsChangedEvent(authentication, tableList.ToArray(), dataSet);
        }
    }
}
