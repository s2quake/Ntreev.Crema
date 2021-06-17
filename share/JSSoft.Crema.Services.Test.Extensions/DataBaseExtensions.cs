using JSSoft.Crema.Services.Random;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class DataBaseExtensions
    {
        public static async Task GenerateStandardAsync(this IDataBase dataBase, Authentication authentication)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                await typeContext.GenerateStandardAsync(authentication);
            }
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                await tableContext.GenerateStandardAsync(authentication);
            }
        }
    }
}