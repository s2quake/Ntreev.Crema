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
            var testContext = serviceProvider.GetService(typeof(ITestContext)) as ITestContext;
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(this);
            if (dataBase is null)
            {
                var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
                var comment = RandomUtility.NextString();
                var authentication = await testContext.LoginRandomAsync(Authority.Admin);
                var dataBaseFlags = this.DataBaseFlags;
                var accessType = this.AccessType;
                var settings = this.Settings;
                dataBase = await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
                if (dataBaseFlags.HasFlag(DataBaseFlags.Loaded) == true)
                {
                    await dataBase.LoadAsync(authentication);
                }
                if (settings != null)
                {
                    await this.Initialize(dataBase, authentication, settings);
                }
                if (dataBaseFlags.HasFlag(DataBaseFlags.Private) == true)
                {
                    await this.InitializeAccessAsync(serviceProvider, dataBase, authentication, accessType);
                }
                if (dataBaseFlags.HasFlag(DataBaseFlags.Locked) == true)
                {
                    await dataBase.LockAsync(authentication, RandomUtility.NextString());
                }
                // await cremaHost.LogoutAsync(authentication);
            }
            return dataBase;
        }

        private async Task Initialize(IDataBase dataBase, Authentication authentication, DataBaseSettings settings)
        {
            var isLoaded = dataBase.IsLoaded;
            if (isLoaded == false)
                await dataBase.LoadAsync(authentication);
            await dataBase.InitializeRandomItemsAsync(authentication, settings);
                if (isLoaded == false)
                await dataBase.UnloadAsync(authentication);
        }

        private async Task InitializeAccessAsync(IServiceProvider serviceProvider, IDataBase dataBase, Authentication authentication, AccessType accessType)
        {
            var isLoaded = dataBase.IsLoaded;
            var userFlags = UserFlags.Offline | UserFlags.NotBanned;
            var excludeList = new List<string>() { authentication.ID };
            if (isLoaded == false)
                await dataBase.LoadAsync(authentication);
            await dataBase.SetPrivateAsync(authentication);
            if (accessType.HasFlag(AccessType.Master) == true)
            {
                foreach (var item in new[] { UserFlags.Admin, UserFlags.Member })
                {
                    var userFilter = new UserFilter(userFlags | item) { ExcludedUserIDs = excludeList.ToArray() };
                    var user = await userFilter.GetUserAsync(serviceProvider);
                    await dataBase.AddAccessMemberAsync(authentication, user.ID, AccessType.Master);
                    excludeList.Add(user.ID);
                }
            }
            if (accessType.HasFlag(AccessType.Developer) == true)
            {
                foreach (var item in new[] { UserFlags.Admin, UserFlags.Member })
                {
                    var userFilter = new UserFilter(userFlags | item) { ExcludedUserIDs = excludeList.ToArray() };
                    var user = await userFilter.GetUserAsync(serviceProvider);
                    await dataBase.AddAccessMemberAsync(authentication, user.ID, AccessType.Developer);
                    excludeList.Add(user.ID);
                }
            }
            if (accessType.HasFlag(AccessType.Editor) == true)
            {
                foreach (var item in new[] { UserFlags.Admin, UserFlags.Member })
                {
                    var userFilter = new UserFilter(userFlags | item) { ExcludedUserIDs = excludeList.ToArray() };
                    var user = await userFilter.GetUserAsync(serviceProvider);
                    await dataBase.AddAccessMemberAsync(authentication, user.ID, AccessType.Editor);
                    excludeList.Add(user.ID);
                }
            }
            if (accessType.HasFlag(AccessType.Guest) == true)
            {
                foreach (var item in new[] { UserFlags.Admin, UserFlags.Member, UserFlags.Guest })
                {
                    var userFilter = new UserFilter(userFlags | item) { ExcludedUserIDs = excludeList.ToArray() };
                    var user = await userFilter.GetUserAsync(serviceProvider);
                    await dataBase.AddAccessMemberAsync(authentication, user.ID, AccessType.Guest);
                    excludeList.Add(user.ID);
                }
            }
            if (isLoaded == false)
                await dataBase.UnloadAsync(authentication);
        }

        public string[] ExcludedDataBaseNames { get; set; }

        public DataBaseSettings Settings { get;set;}

        public DataBaseFlags DataBaseFlags { get; set; }

        public AccessType AccessType { get; set; }

        public Func<IDataBase, bool> Predicate { get; set; }

        public int LogCount { get; set; }

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
            if (this.AccessType != AccessType.None && TestAccessType(dataBase, this.AccessType) == false)
                return false;
            // if (this.IncludedIDs != null && TestEnter(dataBase, this.IncludedIDs) == false)
            //     return false;
            // if (this.ExcludedIDs != null && TestLeft(dataBase, this.ExcludedIDs) == false)
            //     return false;
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

        private static bool TestAccessType(IDataBase dataBase, AccessType accessType)
        {
            if (dataBase.IsPrivate == true)
            {
                if (accessType.HasFlag(AccessType.Master) == true && dataBase.AccessInfo.Members.Any(item => item.AccessType == AccessType.Master) == false)
                    return false;
                if (accessType.HasFlag(AccessType.Developer) == true && dataBase.AccessInfo.Members.Any(item => item.AccessType == AccessType.Developer) == false)
                    return false;
                if (accessType.HasFlag(AccessType.Editor) == true && dataBase.AccessInfo.Members.Any(item => item.AccessType == AccessType.Editor) == false)
                    return false;
                if (accessType.HasFlag(AccessType.Guest) == true && dataBase.AccessInfo.Members.Any(item => item.AccessType == AccessType.Guest) == false)
                    return false;
            }
            return true;
        }

        // private static bool TestEnter(IDataBase dataBase, string[] includedIDs)
        // {
        //     foreach (var item in dataBase.AuthenticationInfos)
        //     {
        //         if (includedIDs.Contains(item.ID) == true)
        //             return true;
        //     }
        //     return false;
        // }

        // private static bool TestLeft(IDataBase dataBase, string[] excludedIDs)
        // {
        //     foreach (var item in dataBase.AuthenticationInfos)
        //     {
        //         if (excludedIDs.Contains(item.ID) == true)
        //             return false;
        //     }
        //     return true;
        // }
    }
}
