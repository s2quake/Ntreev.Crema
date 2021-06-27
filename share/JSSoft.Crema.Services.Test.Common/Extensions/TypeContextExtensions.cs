using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common.Extensions
{
    static class TypeContextExtensions
    {
        public static async Task GenerateStandardAsync(this ITypeContext context, Authentication authentication)
        {
            var root = context.Root;
            {
                await root.GenerateStandardTypeAsync(authentication);
                await root.GenerateStandardFlagsAsync(authentication);
            }

            var category = await root.AddNewCategoryAsync(authentication);
            {
                await category.GenerateStandardTypeAsync(authentication);
                await category.GenerateStandardFlagsAsync(authentication);
            }

            var subCategory = await category.AddNewCategoryAsync(authentication);
            {
                await subCategory.GenerateStandardTypeAsync(authentication);
                await subCategory.GenerateStandardFlagsAsync(authentication);
            }
        }
    }
}
