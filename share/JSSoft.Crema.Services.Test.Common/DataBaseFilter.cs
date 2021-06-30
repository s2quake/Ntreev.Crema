using JSSoft.Crema.Services;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Common.Extensions;

namespace JSSoft.Crema.Services.Test.Common
{
    public class DataBaseFilter
    {
        public DataBaseFilter()
        {
        }

        public DataBaseFilter(DataBaseFlags dataBaseFlags)
        {
            this.DataBaseFlags = dataBaseFlags;
        }

        public async Task<IDataBase> GetDataBaseAsync(IServiceProvider serviceProvider)
        {
            var cremaHost = serviceProvider.GetService(typeof(ICremaHost)) as ICremaHost;
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(this);
            if (dataBase is null)
            {
                var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
                var comment = RandomUtility.NextString();
                var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
                var dataBaseFlags = this.DataBaseFlags;
                dataBase = await dataBaseContext.AddNewDataBaseAsync(Authentication.System, dataBaseName, comment);
                if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true)
                    await dataBase.LoadAsync(authentication);
                if (dataBaseFlags.HasFlag(DataBaseFlags.Private) == true)
                    await dataBase.SetPrivateAsync(authentication);
                if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true)
                    await dataBase.LockAsync(authentication, RandomUtility.NextString());
                await cremaHost.LogoutAsync(authentication);
            }
            return dataBase;
        }

        public string[] ExcludedDataBaseNames { get; set; }

        public DataBaseFlags DataBaseFlags { get; set; }

        public Func<IDataBase, bool> Predicate { get; set; }

        public static implicit operator Func<IDataBase, bool>(DataBaseFilter filter)
        {
            return filter.PredicateFunc;
        }

        private bool PredicateFunc(IDataBase dataBase)
        {
            if (this.DataBaseFlags != DataBaseFlags.None && TestFlags(dataBase, this.DataBaseFlags) == false)
                return false;
            if (this.ExcludedDataBaseNames != null && this.ExcludedDataBaseNames.Contains(dataBase.Name) == true)
                return false;
            if (this.Predicate != null && this.Predicate(dataBase) == false)
                return false;
            return true;
        }

        private static bool TestFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
        {
            return TestDataBaseStateFlags(dataBase, dataBaseFlags) && TestPublicFlags(dataBase, dataBaseFlags) && TestLockFlags(dataBase, dataBaseFlags);
        }

        private static bool TestDataBaseStateFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
        {
            if (dataBase.DataBaseFlags != DataBaseFlags.NotLoaded || dataBase.DataBaseFlags != DataBaseFlags.Loaded)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.NotLoaded) == true && dataBase.DataBaseState != DataBaseState.None)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true && dataBase.DataBaseState != DataBaseState.Loaded)
                return false;
            return true;
        }

        private static bool TestPublicFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
        {
            if (dataBaseFlags.HasFlag(DataBaseFlags.Private) == true && dataBase.AccessInfo.IsPrivate == false)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.Public) == true && dataBase.AccessInfo.IsPublic == false)
                return false;
            return true;
        }

        private static bool TestLockFlags(IDataBase dataBase, DataBaseFlags dataBaseFlags)
        {
            if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true && dataBase.LockInfo.IsLocked == false)
                return false;
            if (dataBaseFlags.HasFlag(DataBaseFlags.NotLocked) == true && dataBase.LockInfo.IsLocked == true)
                return false;
            return true;
        }
    }
}
