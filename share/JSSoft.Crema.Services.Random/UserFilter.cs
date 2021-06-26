using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Random
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

        public static UserFilter FromExcludedUserIDs(params string[] userIDs)
        {
            return new UserFilter()
            {
                ExcludedUserIDs = userIDs
            };
        }

        public string[] ExcludedUserIDs { get; set; }

        public UserFlags UserFlags { get; set; }

        public Func<IUser, bool> Predicate { get; set; }

        public static implicit operator Func<IUser, bool>(UserFilter filter)
        {
            return filter.PredicateFunc;
        }

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
    }
}
