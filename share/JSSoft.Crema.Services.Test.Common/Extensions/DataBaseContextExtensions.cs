using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common.Extensions
{
    static class DataBaseContextExtensions
    {
        public static async Task<IDataBase[]> GenerateDataBasesAsync(this IDataBaseContext dataBaseContext, Authentication authentication, int count)
        {
            var itemList = new List<IDataBase>(count);
            for (var i = 0; i < count; i++)
            {
                var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync(RandomUtility.NextName());
                var comment = RandomUtility.NextString();
                var item = await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
                itemList.Add(item);
            }
            return itemList.ToArray();
        }
    }
}
