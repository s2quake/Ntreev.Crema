using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceModel
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class DispatcherScopeAttribute : Attribute
    {
        private readonly string scopeTypeName;
        private Type scopeType;

        public DispatcherScopeAttribute(Type scopeType)
        {
            this.scopeType = scopeType;
            this.scopeTypeName = scopeType.AssemblyQualifiedName;
        }

        public DispatcherScopeAttribute(string scopeTypeName)
        {
            this.scopeTypeName = scopeTypeName;
        }

        public string ScopeTypeName
        {
            get { return this.scopeTypeName; }
        }

        internal Type ScopeType
        {
            get
            {
                if (this.scopeType == null)
                {
                    this.scopeType = Type.GetType(this.scopeTypeName);
                }
                return this.scopeType;
            }
        }
    }
}
