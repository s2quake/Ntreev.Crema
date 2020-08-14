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

using Ntreev.Crema.Data;
using Ntreev.Library;
using Ntreev.Library.Random;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class TableTemplateExtensions
    {
        static TableTemplateExtensions()
        {
            MinColumnCount = 1;
            MaxColumnCount = 20;
        }

        public static async Task InitializeRandomAsync(this ITableTemplate template, Authentication authentication)
        {
            var tableName = RandomUtility.NextIdentifier();
            await template.SetTableNameAsync(authentication, tableName);
            if (RandomUtility.Within(50) == true)
                await template.SetTagsAsync(authentication, (TagInfo)TagInfoUtility.Names.Random());
            if (RandomUtility.Within(50) == true)
                await template.SetCommentAsync(authentication, RandomUtility.NextString());
            await template.AddRandomColumnsAsync(authentication);
        }

        public static async Task<ITableColumn> AddRandomColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var column = await template.AddNewAsync(authentication);
            await column.InitializeRandomAsync(authentication);
            await template.EndNewAsync(authentication, column);
            return column;
        }

        public static async Task RemoveRandomColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var column = template.RandomOrDefault();
            await column?.DeleteAsync(authentication);
        }

        public static async Task ModifyRandomColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var column = template.RandomOrDefault();
            await column?.ModifyRandomValueAsync(authentication);
        }

        public static Task AddRandomColumnsAsync(this ITableTemplate template, Authentication authentication)
        {
            return AddRandomColumnsAsync(template, authentication, RandomUtility.Next(MinColumnCount, MaxColumnCount));
        }

        public static async Task AddRandomColumnsAsync(this ITableTemplate template, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await AddRandomColumnAsync(template, authentication);
            }
        }

        public static int MinColumnCount { get; set; }

        public static int MaxColumnCount { get; set; }
    }
}
