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

using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles.TypeTemplate
{
    static class TemplateEditor
    {
        public static async Task<bool> EditAsync(ITypeTemplate template, Authentication authentication)
        {
            var memberCount = template.Dispatcher.Invoke(() => template.Count);
            var memberList = new List<JsonTypeMemberInfos.ItemInfo>(memberCount);
            var idToMember = new Dictionary<Guid, ITypeMember>(memberCount);

            template.Dispatcher.Invoke(() =>
            {
                foreach (var item in template)
                {
                    var member = new JsonTypeMemberInfos.ItemInfo()
                    {
                        ID = Guid.NewGuid(),
                        Name = item.Name,
                        Value = item.Value,
                        Comment = item.Comment,
                    };
                    idToMember.Add(member.ID, item);
                    memberList.Add(member);
                }
            });

            var members = new JsonTypeMemberInfos() { Items = memberList.ToArray() };

            using (var editor = new JsonEditorHost(members))
            {
                if (editor.Execute() == false)
                    return false;

                members = editor.Read<JsonTypeMemberInfos>();
            }

            var terminal = new Terminal();
            var key = terminal.ReadKey("do you want to save changes?(Y/N)", ConsoleKey.Y, ConsoleKey.N);
            if (key != ConsoleKey.Y)
                return false;

            //template.Dispatcher.Invoke(() =>
            //{
            foreach (var item in idToMember.Keys.ToArray())
            {
                if (members.Items.Any(i => i.ID == item) == false)
                {
                    var member = idToMember[item];
                    await member.DeleteAsync(authentication);
                    idToMember.Remove(item);
                }
            }

            for (var i = 0; i < members.Items.Length; i++)
            {
                var item = members.Items[i];
                if (item.ID == Guid.Empty)
                {
                    var member = await template.AddNewAsync(authentication);
                    await member.SetNameAsync(authentication, item.Name);
                    await member.SetValueAsync(authentication, item.Value);
                    await member.SetCommentAsync(authentication, item.Comment);
                    await template.EndNewAsync(authentication, member);
                    item.ID = Guid.NewGuid();
                    idToMember.Add(item.ID, member);
                    members.Items[i] = item;
                }
                else if (idToMember.ContainsKey(item.ID) == true)
                {
                    var member = idToMember[item.ID];
                    if (member.Name != item.Name)
                        await member.SetNameAsync(authentication, item.Name);
                    if (member.Value != item.Value)
                        await member.SetValueAsync(authentication, item.Value);
                    if (member.Comment != item.Comment)
                        await member.SetCommentAsync(authentication, item.Comment);
                }
                else
                {
                    throw new InvalidOperationException($"{item.ID} is not existed member.");
                }
            }

            for (var i = 0; i < members.Items.Length; i++)
            {
                var item = members.Items[i];
                var member = idToMember[item.ID];
                await member.SetIndexAsync(authentication, i);
            }
            await template.EndEditAsync(authentication);
            //});

            return true;
        }
    }
}
