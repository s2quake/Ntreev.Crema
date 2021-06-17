using JSSoft.Crema.Data;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class TableExtensions
    {
        public static async Task<ITable> GenerateStandardChildAsync(this ITable table, Authentication authentication, string prefix, IEnumerable<string> keyTypes, IEnumerable<string> columnTypes)
        {
            var typeCollection = table.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var tableName = string.Join("_", EnumerableUtility.Friends(prefix, keyTypes));

            if (table.Childs.ContainsKey(tableName) == true)
                return null;

            var template = await table.NewTableAsync(authentication);
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
        }

        public static async Task GenerateStandardChildAsync(this ITable table, Authentication authentication)
        {
            var typeCollection = table.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var types1 = typeCollection.Select(item => item.Name).ToArray();
            var types2 = CremaDataTypeUtility.GetBaseTypeNames();
            var allTypes = types1.Concat(types2);
            var allKeyTypes = types1.Concat(types2).Where(item => item != typeof(bool).GetTypeName());

            var prefixes = new string[] { "SingleKey", "DoubleKey", "TripleKey", "QuadraKey", };

            for (int i = 0; i < prefixes.Length; i++)
            {
                var query = allKeyTypes.Permutations(i + 1);
                var keys = query.Random();
                var columns = allTypes.Except(keys);
                await table.GenerateStandardChildAsync(authentication, "Child_" + prefixes[i], keys, columns);
            }
        }

        public static async Task GenerateStandardContentAsync(this ITable table, Authentication authentication)
        {
            var content = table.Content;

            await content.BeginEditAsync(authentication);
            await content.EnterEditAsync(authentication);

            await content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 1000));

            foreach (var item in content.Tables)
            {
                await item.Content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 100));
            }

            try
            {
                await content.LeaveEditAsync(authentication);
                await content.EndEditAsync(authentication);
            }
            catch
            {
                await content.CancelEditAsync(authentication);
            }
        }
    }
}
