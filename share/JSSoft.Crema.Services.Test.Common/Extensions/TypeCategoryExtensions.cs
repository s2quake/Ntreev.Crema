using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common.Extensions
{
    static class TypeCategoryExtensions
    {
        public static async Task GenerateStandardTypeAsync(this ITypeCategory category, Authentication authentication)
        {
            var template = await category.NewTypeAsync(authentication);
            await template.SetIsFlagAsync(authentication, false);
            await template.SetCommentAsync(authentication, "Standard Type");

            var az = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i).ToArray();

            await template.AddMemberAsync(authentication, "None", 0, "None Value");
            for (int i = 0; i < az.Length; i++)
            {
                await template.AddMemberAsync(authentication, az[i].ToString(), (long)i + 1, az[i] + " Value");
            }

            await template.EndEditAsync(authentication);
        }

        public static async Task GenerateStandardFlagsAsync(this ITypeCategory category, Authentication authentication)
        {
            var types = category.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var typeNames = await types.Dispatcher.InvokeAsync(() => types.Select(item => item.Name).ToArray());
            var newName = NameUtility.GenerateNewName("Flag", typeNames);
            var template = await category.NewTypeAsync(authentication);
            await template.SetTypeNameAsync(authentication, newName);
            await template.SetIsFlagAsync(authentication, true);
            await template.SetCommentAsync(authentication, "Standard Flag");

            await template.AddMemberAsync(authentication, "None", 0, "None Value");
            await template.AddMemberAsync(authentication, "A", 1, "A Value");
            await template.AddMemberAsync(authentication, "B", 2, "B Value");
            await template.AddMemberAsync(authentication, "C", 4, "C Value");
            await template.AddMemberAsync(authentication, "D", 8, "D Value");
            await template.AddMemberAsync(authentication, "AC", 1 | 4, "AC Value");
            await template.AddMemberAsync(authentication, "ABC", 1 | 2 | 4, "AC Value");
            await template.AddMemberAsync(authentication, "BD", 2 | 8, "AC Value");
            await template.AddMemberAsync(authentication, "All", 1 | 2 | 4 | 8, "All Value");

            await template.EndEditAsync(authentication);
        }
    }
}
