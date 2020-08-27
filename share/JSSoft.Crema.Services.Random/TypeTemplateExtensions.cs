//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using JSSoft.Library.Random;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class TypeTemplateExtensions
    {
        static TypeTemplateExtensions()
        {
            MinMemberCount = 5;
            MaxMemberCount = 50;
        }

        public static async Task InitializeRandomAsync(this ITypeTemplate template, Authentication authentication)
        {
            var typeName = RandomUtility.NextIdentifier();
            await template.SetTypeNameAsync(authentication, typeName);
            if (RandomUtility.Within(50) == true)
                await template.SetIsFlagAsync(authentication, RandomUtility.NextBoolean());
            if (RandomUtility.Within(50) == true)
                await template.SetCommentAsync(authentication, RandomUtility.NextString());
            await template.AddRandomMembersAsync(authentication);
        }

        public static async Task<ITypeMember> AddRandomMemberAsync(this ITypeTemplate template, Authentication authentication)
        {
            var member = await template.AddNewAsync(authentication);
            await member.InitializeRandomAsync(authentication);
            await template.EndNewAsync(authentication, member);
            return member;
        }

        public static async Task RemoveRandomMemberAsync(this ITypeTemplate template, Authentication authentication)
        {
            var member = template.RandomOrDefault();
            await member?.DeleteAsync(authentication);
        }

        public static async Task ModifyRandomMemberAsync(this ITypeTemplate template, Authentication authentication)
        {
            var member = template.RandomOrDefault();
            await member?.ModifyRandomValueAsync(authentication);
        }

        public static Task AddRandomMembersAsync(this ITypeTemplate template, Authentication authentication)
        {
            return AddRandomMembersAsync(template, authentication, RandomUtility.Next(MinMemberCount, MaxMemberCount));
        }

        public static async Task AddRandomMembersAsync(this ITypeTemplate template, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await AddRandomMemberAsync(template, authentication);
            }
        }

        //public static object GetRandomValue(ITypeCollection types, ColumnInfo columnInfo)
        //{
        //    if (columnInfo.AllowNull == true && RandomUtility.Next(4) == 0)
        //    {
        //        return null;
        //    }
        //    else if (CremaDataTypeUtility.IsBaseType(columnInfo.DataType))
        //    {
        //        return GetRandomValue(columnInfo.DataType);
        //    }
        //    else
        //    {
        //        var itemName = new ItemName(columnInfo.DataType);
        //        return GetRandomValue(types[itemName.Name]);
        //    }
        //}

        //public static object GetRandomValue(this IType type)
        //{
        //    var typeInfo = type.TypeInfo;
        //    if (typeInfo.Members.Length == 0)
        //        throw new Exception(type.Name);

        //    if (typeInfo.IsFlag == true)
        //    {
        //        long value = 0;
        //        int count = RandomUtility.Next(1, typeInfo.Members.Length);
        //        for (var i = 0; i < count; i++)
        //        {
        //            var index = RandomUtility.Next(typeInfo.Members.Length);
        //            value |= typeInfo.Members[index].Value;
        //        }
        //        var textvalue = typeInfo.ConvertToString(value);
        //        if (textvalue == string.Empty)
        //            throw new Exception();
        //        return textvalue;
        //    }
        //    else
        //    {
        //        return typeInfo.Members.Random().Name;
        //    }
        //}

        //public static object GetRandomValue(string typeName)
        //{
        //    var type = CremaDataTypeUtility.GetType(typeName);
        //    return RandomUtility.Next(type);
        //}

        public static int MinMemberCount { get; set; }

        public static int MaxMemberCount { get; set; }
    }
}
