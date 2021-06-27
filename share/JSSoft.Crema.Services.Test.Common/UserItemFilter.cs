using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common
{
    public class UserItemFilter
    {
        public UserItemFilter()
        {
        }

        public UserItemFilter(Type type)
        {
            this.Type = type;
        }

        public UserItemFilter(Type type, Func<IUserItem, bool> predicate)
        {
            this.Type = type;
            this.Predicate = predicate;
        }
        public async Task<IUserItem> GetUserItemAsync(IServiceProvider serviceProvider)
        {
            var cremaHost = serviceProvider.GetService(typeof(ICremaHost)) as ICremaHost;
            var userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var userItem = await userContext.GetRandomUserItemAsync(this);
            if (userItem is null)
            {
                var s = RandomUtility.Within(50);
                if (this.Type == typeof(IUser) || (this.Type == null && s == true))
                {
                    var userFilter = (UserFilter)this;
                    var user = await userFilter.GetUserAsync(serviceProvider);
                    userItem = user as IUserItem;
                }
                else if (this.Type == typeof(IUserCategory) || (this.Type == null && s == false))
                {
                    var categoryFilter = (UserCategoryFilter)this;
                    var category = await categoryFilter.GetUserCategoryAsync(serviceProvider);
                    userItem = category as IUserItem;
                }
            }
            return userItem;
        }

        public Type Type { get; set; }

        public bool HasParent { get; set; }

        public bool HasChilds { get; set; }

        public bool IsLeaf { get; set; }

        public IUserItem TargetToMove { get; set; }

        public IUserItem[] ExcludedItems { get; set; }

        public Func<IUserItem, bool> Predicate { get; set; }

        public static implicit operator Func<IUserItem, bool>(UserItemFilter filter)
        {
            return filter.PredicateFunc;
        }

        public static implicit operator UserCategoryFilter(UserItemFilter filter)
        {
            var s = RandomUtility.Within(50);
            var userCategoryFilter = new UserCategoryFilter
            {
                HasParent = filter.HasParent,
                HasCategories = filter.HasChilds == true && s == true,
                HasUsers = filter.HasChilds == true && s == true,
                IsLeaf = filter.IsLeaf,
                CategoryToMove = filter.TargetToMove as IUserCategory,
                ExcludedItems = filter.ExcludedItems,
                Predicate = filter.Predicate != null ? (item) => filter.Predicate(item as IUserItem) : (item) => true,
            };
            return userCategoryFilter;
        }

        public static implicit operator UserFilter(UserItemFilter filter)
        {
            var userFilter = new UserFilter
            {
                ExcludedUserIDs = filter.ExcludedItems != null ? filter.ExcludedItems.Where(item => item is IUser).Select(item => (item as IUser).ID).ToArray() : null,
                Predicate = filter.Predicate != null ? (item) => filter.Predicate(item as IUserItem) : (item) => true,
            };
            return userFilter;
        }

        private bool PredicateFunc(IUserItem userItem)
        {
            if (this.Type != null && this.Type.IsAssignableFrom(userItem.GetType()) == false)
                return false;
            if (this.HasParent == true && userItem.Parent == null)
                return false;
            if (this.HasChilds == true && userItem.Childs.Any() == false)
                return false;
            if (this.IsLeaf == true && userItem.Childs.Any() == true)
                return false;
            if (this.TargetToMove != null && CanMove(this.TargetToMove, userItem.Path) == false)
                return false;
            if (this.ExcludedItems != null && this.ExcludedItems.Contains(userItem) == true)
                return false;
            if (this.Predicate != null && this.Predicate(userItem) == false)
                return false;

            return true;
        }

        private static bool CanMove(IUserItem userItem, string parentPath)
        {
            if (userItem.Parent == null)
                return false;
            if (userItem.Parent.Path == parentPath)
                return false;
            if (NameValidator.VerifyCategoryPath(parentPath) == false)
                return false;
            return parentPath.StartsWith(userItem.Path) == false;
        }
    }
}
