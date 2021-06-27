using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Test.Common;
using JSSoft.Crema.Services.Test.Common.Extensions;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common
{
    public class UserFilter
    {
        public UserFilter()
        {
        }

        public UserFilter(UserFlags userFlags)
        {
            this.UserFlags = userFlags;
        }

        public UserFilter(UserFlags userFlags, Func<IUser, bool> predicate)
        {
            this.UserFlags = userFlags;
            this.Predicate = predicate;
        }

        public async Task<IUser> GetUserAsync(IServiceProvider serviceProvider)
        {
            var userFlags = this.UserFlags;
            var cremaHost = serviceProvider.GetService(typeof(ICremaHost)) as ICremaHost;
            var userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var user = await userCollection.GetRandomUserAsync(userFlags, this);
            if (user is null)
            {
                var authority = GetAuthorities(userFlags).Random();
                var userStates = SelectUserState(userFlags);
                var banStates = SelectBanState(userFlags);
                var category = await userCategoryCollection.GetRandomUserCategoryAsync();
                var newUser = await category.GenerateUserAsync(Authentication.System, authority);
                var password = newUser.GetPassword();
                if (userStates.Any() == true && userStates.Random() == UserState.Online)
                    await cremaHost.LoginAsync(newUser.ID, password);
                if (banStates.Any() == true && banStates.Random() == true)
                    await newUser.BanAsync(Authentication.System, RandomUtility.NextString());
                return newUser;
            }
            return user;
        }

        public string[] ExcludedUserIDs { get; set; }

        public UserFlags UserFlags { get; set; }

        public Func<IUser, bool> Predicate { get; set; }

        public static implicit operator Func<IUser, bool>(UserFilter filter) => filter.PredicateFunc;

        public static UserFilter Empty { get; } = new UserFilter();

        private bool PredicateFunc(IUser user)
        {
            if (this.UserFlags != UserFlags.None && TestFlags(user, this.UserFlags) == false)
                return false;
            if (this.ExcludedUserIDs != null && this.ExcludedUserIDs.Contains(user.ID) == true)
                return false;
            if (this.Predicate != null && this.Predicate(user) == false)
                return false;

            return true;
        }

        private static bool TestFlags(IUser user, UserFlags userFlags)
        {
            return TestAuthorityFlags(user, userFlags) && TestUserStateFlags(user, userFlags) && TestBanInfoFlags(user, userFlags);
        }

        private static bool TestAuthorityFlags(IUser user, UserFlags userFlags)
        {
            var mask = userFlags & (UserFlags.Admin | UserFlags.Member | UserFlags.Guest);
            if (mask.HasFlag(UserFlags.Admin) == true && user.Authority == Authority.Admin)
                return true;
            if (mask.HasFlag(UserFlags.Member) == true && user.Authority == Authority.Member)
                return true;
            if (mask.HasFlag(UserFlags.Guest) == true && user.Authority == Authority.Guest)
                return true;
            return mask == UserFlags.None;
        }

        private static bool TestUserStateFlags(IUser user, UserFlags userFlags)
        {
            if (userFlags.HasFlag(UserFlags.Offline) == true && user.UserState != UserState.None)
                return false;
            if (userFlags.HasFlag(UserFlags.Online) == true && user.UserState != UserState.Online)
                return false;
            return true;
        }

        private static bool TestBanInfoFlags(IUser user, UserFlags userFlags)
        {
            if (userFlags.HasFlag(UserFlags.NotBanned) == true && user.BanInfo.IsBanned == true)
                return false;
            if (userFlags.HasFlag(UserFlags.Banned) == true && user.BanInfo.IsNotBanned == true)
                return false;
            return true;
        }

        private static UserState[] SelectUserState(UserFlags userFlags)
        {
            var userStateList = new List<UserState>();
            var userStateFlag = userFlags & (UserFlags.Online | UserFlags.Offline);
            if (userFlags.HasFlag(UserFlags.Online) == true)
            {
                userStateList.Add(UserState.Online);
            }
            if (userFlags.HasFlag(UserFlags.Offline) == true)
            {
                userStateList.Add(UserState.None);
            }
            return userStateList.ToArray();
        }

        private static bool[] SelectBanState(UserFlags userFlags)
        {
            var banStateList = new List<bool>();
            var banStateFlag = userFlags & (UserFlags.NotBanned | UserFlags.Banned);
            if (userFlags.HasFlag(UserFlags.NotBanned) == true)
            {
                banStateList.Add(false);
            }
            if (userFlags.HasFlag(UserFlags.Banned) == true)
            {
                banStateList.Add(true);
            }
            return banStateList.ToArray();
        }

        private static Authority[] GetAuthorities(UserFlags userFlags)
        {
            var authorityList = new List<Authority>();
            var authorityFlag = userFlags & (UserFlags.Admin | UserFlags.Member | UserFlags.Guest);
            if (userFlags.HasFlag(UserFlags.Admin) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Admin);
            }
            if (userFlags.HasFlag(UserFlags.Member) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Member);
            }
            if (userFlags.HasFlag(UserFlags.Guest) == true || authorityFlag == UserFlags.None)
            {
                authorityList.Add(Authority.Guest);
            }
            return authorityList.ToArray();
        }
    }
}
