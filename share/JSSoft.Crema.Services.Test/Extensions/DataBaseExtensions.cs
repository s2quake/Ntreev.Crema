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
            await dataBase.TypeContext.GenerateStandardAsync(authentication);
            await dataBase.TableContext.GenerateStandardAsync(authentication);
        }
    }
}
