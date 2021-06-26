using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Random
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

        public Type Type { get; set; }

        public bool HasParent { get; set; }

        public bool HasChilds { get; set; }

        public bool IsLeaf { get; set; }

        public IUserItem[] ExcludedParents { get; set; }

        public static implicit operator Func<IUserItem, bool>(UserItemFilter filter)
        {
            return filter.Predicate;
        }

        private bool Predicate(IUserItem userItem)
        {
            if (this.Type != null && this.Type.IsAssignableFrom(userItem.GetType()) == false)
                return false;
            if (this.HasParent == true && userItem.Parent == null)
                return false;
            if (this.HasChilds == true && userItem.Childs.Any() == false)
                return false;
            if (this.IsLeaf == true && userItem.Childs.Any() == true)
                return false;
            if (this.ExcludedParents != null && this.ExcludedParents.Contains(userItem.Parent) == true)
                return false;

            return true;
        }
    }
}
