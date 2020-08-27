// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public static class IDomainExtensions
    {
        public static Task NewRowAsync(this IDomain domain, Authentication authentication, string tableName, object[] fields)
        {
            var row = new DomainRowInfo()
            {
                TableName = tableName,
                Fields = fields,
            };

            return domain.NewRowAsync(authentication, new DomainRowInfo[] { row });
        }

        public static Task BeginEditAsync(this IDomain domain, Authentication authentication, object item, string fieldName)
        {
            var location = new DomainLocationInfo()
            {
                TableName = CremaDataRowUtility.GetTableName(item),
                Keys = CremaDataRowUtility.GetKeys(item),
                ColumnName = fieldName,
            };
            return domain.BeginUserEditAsync(authentication, location);
        }

        public static Task RemoveRowAsync(this IDomain domain, Authentication authentication, string tableName, object[] keys)
        {
            var row = new DomainRowInfo()
            {
                TableName = tableName,
                Keys = keys,
            };
            return domain.RemoveRowAsync(authentication, new DomainRowInfo[] { row });
        }

        public static Task RemoveRowAsync(this IDomain domain, Authentication authentication, object item)
        {
            var row = new DomainRowInfo()
            {
                TableName = CremaDataRowUtility.GetTableName(item),
                Keys = CremaDataRowUtility.GetKeys(item),
            };
            return domain.RemoveRowAsync(authentication, new DomainRowInfo[] { row });
        }

        public static Task RemoveRowsAsync(this IDomain domain, Authentication authentication, IEnumerable items)
        {
            var query = from object item in items
                        select new DomainRowInfo()
                        {
                            TableName = CremaDataRowUtility.GetTableName(item),
                            Keys = CremaDataRowUtility.GetKeys(item),
                        };

            return domain.RemoveRowAsync(authentication, query.ToArray());
        }

        public static Task SetLocationAsync(this IDomain domain, Authentication authentication, object item, string fieldName)
        {
            var location = new DomainLocationInfo()
            {
                TableName = CremaDataRowUtility.GetTableName(item),
                Keys = CremaDataRowUtility.GetKeys(item),
                ColumnName = fieldName,
            };
            return domain.SetUserLocationAsync(authentication, location);
        }

        public static Task SetLocationAsync(this IDomain domain, Authentication authentication)
        {
            return domain.SetUserLocationAsync(authentication, DomainLocationInfo.Empty);
        }

        public static Task SetRowAsync(this IDomain domain, Authentication authentication, string tableName, object[] keys, object[] fields)
        {
            var rowValue = new DomainRowInfo()
            {
                TableName = tableName,
                Keys = keys,
                Fields = fields,
            };
            return domain.SetRowAsync(authentication, new DomainRowInfo[] { rowValue });
        }

        public static Task SetRowAsync(this IDomain domain, Authentication authentication, object item, string fieldName, object value)
        {
            var rowValue = new DomainRowInfo()
            {
                TableName = CremaDataRowUtility.GetTableName(item),
                Keys = CremaDataRowUtility.GetKeys(item),
                Fields = CremaDataRowUtility.GetFields(item, fieldName, value),
            };
            return domain.SetRowAsync(authentication, new DomainRowInfo[] { rowValue });
        }
    }
}
