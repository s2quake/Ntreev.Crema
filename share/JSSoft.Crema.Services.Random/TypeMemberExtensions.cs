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
    public static class TypeMemberExtensions
    {
        public static async Task InitializeRandomAsync(this ITypeMember typeMember, Authentication authentication)
        {
            if (typeMember.Template.IsFlag == true)
                await typeMember.SetValueAsync(authentication, RandomUtility.NextBit());
            else if (RandomUtility.Within(95) == true)
                await typeMember.SetValueAsync(authentication, (long)typeMember.Template.Count);
            else
                await typeMember.SetValueAsync(authentication, RandomUtility.NextLong(long.MaxValue));

            if (RandomUtility.Within(50) == true)
                await typeMember.SetCommentAsync(authentication, RandomUtility.NextString());
        }

        public static async Task ModifyRandomValueAsync(this ITypeMember typeMember, Authentication authentication)
        {
            if (RandomUtility.Within(75) == true)
            {
                await SetRandomNameAsync(typeMember, authentication);
            }
            else if (RandomUtility.Within(75) == true)
            {
                await SetRandomValueAsync(typeMember, authentication);
            }
            else
            {
                await SetRandomCommentAsync(typeMember, authentication);
            }
        }

        public static async Task ExecuteRandomTaskAsync(this ITypeMember typeMember, Authentication authentication)
        {
            if (RandomUtility.Within(75) == true)
            {
                await SetRandomNameAsync(typeMember, authentication);
            }
            else if (RandomUtility.Within(75) == true)
            {
                await SetRandomValueAsync(typeMember, authentication);
            }
            else
            {
                await SetRandomCommentAsync(typeMember, authentication);
            }
        }

        public static async Task SetRandomNameAsync(this ITypeMember typeMember, Authentication authentication)
        {
            var newName = RandomUtility.NextIdentifier();
            await typeMember.SetNameAsync(authentication, newName);
        }

        public static async Task SetRandomValueAsync(this ITypeMember typeMember, Authentication authentication)
        {
            if (typeMember.Template.IsFlag == true)
            {
                await typeMember.SetValueAsync(authentication, RandomUtility.NextBit());
            }
            else
            {
                await typeMember.SetValueAsync(authentication, RandomUtility.NextLong(long.MaxValue));
            }
        }

        public static async Task SetRandomCommentAsync(this ITypeMember typeMember, Authentication authentication)
        {
            if (RandomUtility.Within(50) == true)
            {
                await typeMember.SetCommentAsync(authentication, RandomUtility.NextString());
            }
            else
            {
                await typeMember.SetCommentAsync(authentication, string.Empty);
            }
        }
    }
}
