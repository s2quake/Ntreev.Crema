using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class TableCategoryExtensions
    {
        public static async Task<ITable> GenerateStandardTableAsync(this ITableCategory category, Authentication authentication, string prefix, IEnumerable<string> keyTypes, IEnumerable<string> columnTypes)
        {
            var tables = category.GetService(typeof(ITableCollection)) as ITableCollection;
            var tableName = string.Join("_", EnumerableUtility.Friends(prefix, keyTypes));

            if (tables.Contains(tableName) == true)
                return null;

            var template = await category.NewTableAsync(authentication);
            await template.SetTableNameAsync(authentication, tableName);

            foreach (var item in keyTypes)
            {
                await template.AddKeyAsync(authentication, item, item);
            }

            foreach (var item in columnTypes)
            {
                await template.AddColumnAsync(authentication, item, item);
            }

            try
            {
                await template.EndEditAsync(authentication);
                return template.Target as ITable;
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                return null;
            }

            //var table = template.Table;
            //var content = table.Content;

            //content.BeginEdit(authentication);
            //content.EnterEdit(authentication);
            //try
            //{
            //    content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 1000));
            //    content.LeaveEdit(authentication);
            //    content.EndEdit(authentication);
            //}
            //catch
            //{
            //    content.CancelEdit(authentication);
            //}

            //return table;
        }
    }
}
