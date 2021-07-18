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

using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class TypeContextExtensions
    {
        public static async Task AddRandomItemsAsync(this ITypeContext typeContext, Authentication authentication, DataBaseSettings settings)
        {
            await AddRandomCategoriesAsync(typeContext, authentication, settings);
            await AddRandomTypesAsync(typeContext, authentication, settings);
        }

        public static async Task AddRandomCategoriesAsync(this ITypeContext typeContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TypeContext.MinTypeCategoryCount;
            var maxCount = settings.TypeContext.MaxTypeCategoryCount;
            var count = RandomUtility.Next(minCount, maxCount);
            var typeCategoryCollection = typeContext.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            while (await typeCategoryCollection.GetCountAsync() < count)
            {
                try
                {
                    if (typeContext is null)
                        throw new ArgumentNullException(nameof(typeContext));
                    await typeContext.AddRandomCategoryAsync(authentication);
                }
                catch (System.NullReferenceException e)
                {
                    System.Diagnostics.Debugger.Launch();
                }
            }
        }

        public static Task<ITypeCategory> AddRandomCategoryAsync(this ITypeCategory category, Authentication authentication)
        {
            var categoryName = RandomUtility.NextIdentifier();
            return category.AddNewCategoryAsync(authentication, categoryName);
        }

        public static Task<ITypeCategory> AddRandomCategoryAsync(this ITypeContext typeContext, Authentication authentication)
        {
            if (RandomUtility.Within(33) == true)
            {
                return typeContext.Root.AddRandomCategoryAsync(authentication);
            }
            else
            {
                var category = typeContext.Categories.Random();
                if (GetLevel(category, (i) => i.Parent) > 4)
                    return null;
                return category.AddRandomCategoryAsync(authentication);
            }
        }

        public static async Task AddRandomTypesAsync(this ITypeContext typeContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TypeContext.MinTypeCount;
            var maxCount = settings.TypeContext.MaxTypeCount;
            var count = RandomUtility.Next(minCount, maxCount);
            var typeCollection = typeContext.GetService(typeof(ITypeCollection)) as ITypeCollection;
            while (await typeCollection.GetCountAsync() < count)
            {
                await AddRandomTypeAsync(typeContext, authentication);
            }
        }

        public static Task<IType> AddRandomTypeAsync(this ITypeContext typeContext, Authentication authentication)
        {
            var category = typeContext.Categories.Random();
            return AddRandomTypeAsync(category, authentication);
        }

        public static async Task<IType> AddRandomTypeAsync(this ITypeCategory category, Authentication authentication)
        {
            var template = await category.NewTypeAsync(authentication);
            await template.InitializeRandomAsync(authentication);
            try
            {
                await template.EndEditAsync(authentication);
                return template.Type;
            }
            catch (Exception)
            {
                await template.CancelEditAsync(authentication);
                return null;
            }
        }

        public static Task<ITypeItem> GetRandomTypeItemAsync(this ITypeContext typeContext)
        {
            return GetRandomTypeItemAsync(typeContext, DefaultPredicate);
        }

        public static Task<ITypeItem> GetRandomTypeItemAsync(this ITypeContext typeContext, Func<ITypeItem, bool> predicate)
        {
            return typeContext.Dispatcher.InvokeAsync(() => typeContext.Random(predicate));
        }

        private static bool DefaultPredicate(ITypeItem _) => true;

        private static int GetLevel<T>(T category, Func<T, T> parentFunc)
        {
            var level = 0;
            var parent = parentFunc(category);
            while (parent != null)
            {
                level++;
                parent = parentFunc(parent);
            }
            return level;
        }
    }
}
