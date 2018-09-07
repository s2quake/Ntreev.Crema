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
using Ntreev.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Text;
using Ntreev.Library;
using Ntreev.Crema.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class CremaDataCreator
    {
        public static async Task CreateStandardAsync(this IDataBase dataBase, Authentication authentication)
        {
            var tableContext = dataBase.TableContext;
            await tableContext.Root.AddNewCategoryAsync(authentication, "All");
            await tableContext.Root.AddNewCategoryAsync(authentication, "Client");
            await tableContext.Root.AddNewCategoryAsync(authentication, "Server");
            await tableContext.Root.AddNewCategoryAsync(authentication, "None");
        }

        private static async Task CreateTableAsync(Authentication authentication, ITableCategory category, string name, TagInfo tags)
        {
            var template = await category.NewTableAsync(authentication);

            await template.SetTableNameAsync(authentication, name);
            await template.SetTagsAsync(authentication, tags);
            await template.SetCommentAsync(authentication, $"table-{tags}");

            var key = await template.AddNewAsync(authentication);
            await key.SetNameAsync(authentication, "key_column");
            await key.SetIsKeyAsync(authentication, true);
            await template.EndNewAsync(authentication, key);

            var all = await template.AddNewAsync(authentication);
            await all.SetNameAsync(authentication, "all_column");
            await template.EndNewAsync(authentication, all);

            var server = await template.AddNewAsync(authentication);
            await server.SetNameAsync(authentication, "server_column");
            await template.EndNewAsync(authentication, server);

            var client = await template.AddNewAsync(authentication);
            await client.SetNameAsync(authentication, "client_column");
            await template.EndNewAsync(authentication, client);

            var none = await template.AddNewAsync(authentication);
            await none.SetNameAsync(authentication, "none_column");
            await template.EndNewAsync(authentication, none);

            await template.EndEditAsync(authentication);

            if (template.Target is ITable[] tables)
            {
                var table = tables.First();

                await CreateTableAsync(authentication, table, "child_all", TagInfoUtility.All);
                await CreateTableAsync(authentication, table, "child_server", TagInfoUtility.Server);
                await CreateTableAsync(authentication, table, "child_client", TagInfoUtility.Client);
                await CreateTableAsync(authentication, table, "child_none", TagInfoUtility.Unused);
            }
        }

        private static async Task CreateTableAsync(Authentication authentication, ITable table, string name, TagInfo tags)
        {
            var template = await table.NewTableAsync(authentication);

            await template.SetTableNameAsync(authentication, name);
            await template.SetTagsAsync(authentication, tags);
            await template.SetCommentAsync(authentication, $"table-{tags}");

            var key = await template.AddNewAsync(authentication);
            await key.SetNameAsync(authentication, "key_column");
            await key.SetIsKeyAsync(authentication, true);
            await template.EndNewAsync(authentication, key);

            var all = await template.AddNewAsync(authentication);
            await all.SetNameAsync(authentication, "all_column");
            await template.EndNewAsync(authentication, all);

            var server = await template.AddNewAsync(authentication);
            await server.SetNameAsync(authentication, "server_column");
            await template.EndNewAsync(authentication, server);

            var client = await template.AddNewAsync(authentication);
            await client.SetNameAsync(authentication, "client_column");
            await template.EndNewAsync(authentication, client);

            var none = await template.AddNewAsync(authentication);
            await none.SetNameAsync(authentication, "none_column");
            await template.EndNewAsync(authentication, none);

            await template.EndEditAsync(authentication);
        }
    }
}
