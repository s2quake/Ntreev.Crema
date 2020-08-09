using System;

namespace Ntreev.Crema.ServiceModel
{
    public interface IColumnModel : IDisposable
    {
        Guid ID
        {
            get;
        }

        bool IsKey
        {
            get;
            set;
        }

        bool IsUnique
        {
            get;
            set;
        }

        string Name
        {
            get;
            set;
        }

        string DataType
        {
            get;
            set;
        }

        string DefaultValue
        {
            get;
            set;
        }

        bool AutoIncrement
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        string DataLocation
        {
            get;
            set;
        }

        string Creator
        {
            get;
        }

        DateTime CreatedDateTime
        {
            get;
        }

        string Modifier
        {
            get;
        }

        DateTime ModifiedDateTime
        {
            get;
        }
    }
}
