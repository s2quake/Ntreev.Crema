﻿using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Random
{
    public class UserCategoryFilter
    {
        public UserCategoryFilter()
        {
        }

        public static UserCategoryFilter FromExcludedCategories(params IUserCategory[] categories)
        {
            return new UserCategoryFilter()
            {
                ExcludedCategories = categories
            };
        }

        public static UserCategoryFilter FromExcludedItems(params IUserItem[] userItems)
        {
            return new UserCategoryFilter()
            {
                ExcludedItems = userItems
            };
        }

        public IUserCategory[] ExcludedCategories { get; set; }

        public IUserItem[] ExcludedItems { get; set; }

        public bool HasParent { get; set; }

        public bool HasCategories { get; set; }

        public bool HasUsers { get; set; }

        public bool IsLeaf { get; set; }

        public IUserCategory TargetToMove { get; set; }

        public Func<IUserCategory, bool> Predicate { get; set; }

        public static UserCategoryFilter Empty { get; } = new UserCategoryFilter();

        public static implicit operator Func<IUserCategory, bool>(UserCategoryFilter filter)
        {
            return filter.PredicateFunc;
        }

        private bool PredicateFunc(IUserCategory userCategory)
        {
            if (this.HasParent == true && userCategory.Parent == null)
                return false;
            if (this.HasCategories == true && userCategory.Categories.Any() == false)
                return false;
            if (this.HasUsers == true && userCategory.Users.Any() == false)
                return false;
            if (this.IsLeaf == true && (userCategory.Categories.Any() == true || userCategory.Users.Any() == true))
                return false;
            if (this.TargetToMove != null && this.TargetToMove.CanMove(userCategory.Path) == false)
                return false;
            if (this.ExcludedCategories != null && this.ExcludedCategories.Contains(userCategory) == true)
                return false;
            if (this.ExcludedItems != null && this.ExcludedItems.Contains(userCategory as IUserItem) == true)
                return false;
            if (this.Predicate != null && this.Predicate(userCategory) == false)
                return false;

            return true;
        }
    }
}