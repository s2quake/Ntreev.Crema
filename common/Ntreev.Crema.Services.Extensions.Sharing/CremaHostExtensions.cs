using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Library.Linq;
using Ntreev.Crema.ServiceModel;

namespace Ntreev.Crema.Services.Extensions
{
    public static class CremaHostExtensions
    {
        public static Task<bool> ContainsDataBaseAsync(this ICremaHost cremaHost, string dataBaseName)
        {
            if (cremaHost.GetService(typeof(IDataBaseCollection)) is IDataBaseCollection dataBases)
            {
                return dataBases.Dispatcher.InvokeAsync(() => dataBases.Contains(dataBaseName));
            }
            throw new NotImplementedException();
        }

        public static async Task<bool> ContainsTableAsync(this ICremaHost cremaHost, string dataBaseName, string tableName)
        {
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() => dataBase.TableContext.Tables.Contains(tableName));
        }

        public static async Task<bool> ContainsTableItemAsync(this ICremaHost cremaHost, string dataBaseName, string tableItemPath)
        {
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() => dataBase.TableContext.Contains(tableItemPath));
        }

        public static async Task<bool> ContainsTypeAsync(this ICremaHost cremaHost, string dataBaseName, string typeName)
        {
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() => dataBase.TypeContext.Types.Contains(typeName));
        }

        public static async Task<bool> ContainsTypeItemAsync(this ICremaHost cremaHost, string dataBaseName, string typeItemPath)
        {
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() => dataBase.TypeContext.Contains(typeItemPath));
        }

        public static Task<bool> ContainsDomainAsync(this ICremaHost cremaHost, Guid domainID)
        {
            if (cremaHost.GetService(typeof(IDomainContext)) is IDomainContext domainContext)
            {
                return domainContext.Domains.ContainsAsync(domainID);
            }
            throw new NotImplementedException();
        }

        public static async Task<bool> ContainsDomainUserAsync(this ICremaHost cremaHost, Guid domainID, string userID)
        {
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            var domain = await GetDomainAsync(cremaHost, domainID);
            return await domain.Users.ContainsAsync(userID);
        }

        public static async Task<bool> ContainsUserAsync(this ICremaHost cremaHost, string userID)
        {
            if (cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return await userContext.Users.ContainsAsync(userID);
            }
            throw new NotImplementedException();
        }

        public static async Task<bool> ContainsUserItemAsync(this ICremaHost cremaHost, string categoryPath)
        {
            if (cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return await userContext.Categories.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<IDataBase> GetDataBaseAsync(this ICremaHost cremaHost, string dataBaseName)
        {
            return GetDataBaseAsync(cremaHost, dataBaseName, false);
        }

        public static Task<IDataBase> GetDataBaseAsync(this ICremaHost cremaHost, string dataBaseName, bool isLoaded)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (cremaHost.GetService(typeof(IDataBaseCollection)) is IDataBaseCollection dataBases)
            {
                return dataBases.Dispatcher.InvokeAsync(() =>
                {
                    if (dataBases.Contains(dataBaseName) == false)
                        throw new DataBaseNotFoundException(dataBaseName);
                    var dataBase = dataBases[dataBaseName];
                    if (isLoaded == true && dataBase.IsLoaded == false)
                        throw new InvalidOperationException("database is not loaded");
                    return dataBase;
                });
            }
            throw new NotImplementedException();
        }

        public static async Task<ITable> GetTableAsync(this ICremaHost cremaHost, string dataBaseName, string tableName)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var table = dataBase.TableContext.Tables[tableName];
                if (table == null)
                    throw new TableNotFoundException(tableName);
                return table;
            });
        }

        public static async Task<ITableItem> GetTableItemAsync(this ICremaHost cremaHost, string dataBaseName, string tableItemPath)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (tableItemPath == null)
                throw new ArgumentNullException(nameof(tableItemPath));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var tableItem = dataBase.TableContext[tableItemPath];
                if (tableItem == null)
                    throw new ItemNotFoundException(tableItemPath);
                return tableItem;
            });
        }

        public static async Task<ITableCategory> GetTableCategoryAsync(this ICremaHost cremaHost, string dataBaseName, string categoryPath)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (categoryPath == null)
                throw new ArgumentNullException(nameof(categoryPath));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var category = dataBase.TableContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }

        public static async Task<IType> GetTypeAsync(this ICremaHost cremaHost, string dataBaseName, string typeName)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var type = dataBase.TypeContext.Types[typeName];
                if (type == null)
                    throw new TypeNotFoundException(typeName);
                return type;
            });
        }

        public static async Task<ITypeItem> GetTypeItemAsync(this ICremaHost cremaHost, string dataBaseName, string typeItemPath)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (typeItemPath == null)
                throw new ArgumentNullException(nameof(typeItemPath));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var typeItem = dataBase.TypeContext[typeItemPath];
                if (typeItem == null)
                    throw new ItemNotFoundException(typeItemPath);
                return typeItem;
            });
        }

        public static async Task<ITypeCategory> GetTypeCategoryAsync(this ICremaHost cremaHost, string dataBaseName, string categoryPath)
        {
            if (dataBaseName == null)
                throw new ArgumentNullException(nameof(dataBaseName));
            if (categoryPath == null)
                throw new ArgumentNullException(nameof(categoryPath));
            var dataBase = await GetDataBaseAsync(cremaHost, dataBaseName, true);
            return await dataBase.Dispatcher.InvokeAsync(() =>
            {
                var category = dataBase.TypeContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }

        public static Task<IDomain> GetDomainAsync(this ICremaHost cremaHost, Guid domainID)
        {
            if (cremaHost.GetService(typeof(IDomainContext)) is IDomainContext domainContext)
            {
                return domainContext.Dispatcher.InvokeAsync(() =>
                {
                    if (domainContext.Domains.Contains(domainID) == false)
                        throw new DomainNotFoundException(domainID);
                    return domainContext.Domains[domainID];
                });
            }
            throw new NotImplementedException();
        }

        public static Task<IUser> GetUserAsync(this ICremaHost cremaHost, string userID)
        {
            if (cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return userContext.Dispatcher.InvokeAsync(() =>
                {
                    if (userContext.Users.Contains(userID) == false)
                        throw new UserNotFoundException(userID);
                    return userContext.Users[userID];
                });
            }
            throw new NotImplementedException();
        }

        public static Task<IUserCategory> GetUserCategoryAsync(this ICremaHost cremaHost, string categoryPath)
        {
            if (cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return userContext.Dispatcher.InvokeAsync(() =>
                {
                    if (userContext.Categories.Contains(categoryPath) == false)
                        throw new CategoryNotFoundException(categoryPath);
                    return userContext.Categories[categoryPath];
                });
            }
            throw new NotImplementedException();
        }

        public static Task<IUserItem> GetUserItemAsync(this ICremaHost cremaHost, string userItemPath)
        {
            if (cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return userContext.Dispatcher.InvokeAsync(() =>
                {
                    if (userContext.Contains(userItemPath) == false)
                        throw new ItemNotFoundException(userItemPath);
                    return userContext[userItemPath];
                });
            }
            throw new NotImplementedException();
        }
    }
}
