using JSSoft.Crema.Data;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class TableContextExtensions
    {
        public static async Task GenerateStandardAsync(this ITableContext context, Authentication authentication)
        {
            var typeContext = context.GetService(typeof(ITypeContext)) as ITypeContext;
            var root = context.Root;
            var types1 = typeContext.Types.Select(item => item.Name).ToArray();
            var types2 = CremaDataTypeUtility.GetBaseTypeNames();
            var allTypes = types1.Concat(types2);
            var allKeyTypes = types1.Concat(types2).Where(item => item != typeof(bool).GetTypeName());

            {
                var category = await root.AddNewCategoryAsync(authentication, "SingleKey");
                foreach (var item in allTypes)
                {
                    var table = await category.GenerateStandardTableAsync(authentication, "SingleKey", EnumerableUtility.AsEnumerable(item), allTypes.Where(i => i != item));
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "SingleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "DoubleKey");
                var query = allKeyTypes.Permutations(2);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "DoubleKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "DoubleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "TripleKey");
                var query = allKeyTypes.Permutations(3);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "TripleKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "TripleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "QuadraKey");
                var query = allKeyTypes.Permutations(4);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "QuadraKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "QuadraKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }
        }
    }
}
