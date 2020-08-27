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

using JSSoft.Library;
using JSSoft.Library.Serialization;
using System;

namespace JSSoft.Crema.Services
{
    public interface IUserConfiguration
    {
        void Commit(object target);

        bool Contains(string name);

        object this[string name] { get; set; }

        void Update(object target);
    }

    public static class ICremaConfigurationExtensions
    {
        public static bool TryGetValue<T>(this IUserConfiguration config, Type section, string key, out T value)
        {
#pragma warning disable CS0612 // 'ConfigurationItem'은(는) 사용되지 않습니다.
            var configItem = new ConfigurationItem(section.Name, key);
#pragma warning restore CS0612 // 'ConfigurationItem'은(는) 사용되지 않습니다.
            if (config.Contains(configItem) == true)
            {
                try
                {
                    if (ConfigurationBase.CanSupportType(typeof(T)) == true)
                    {
                        value = (T)config[configItem];
                        return true;
                    }
                    else
                    {
                        var text = (string)config[configItem];
                        value = XmlSerializerUtility.ReadString<T>(text.Decrypt());
                        return true;
                    }
                }
                catch
                {

                }
            }

            value = default(T);
            return false;
        }

        public static void SetValue<T>(this IUserConfiguration config, Type section, string key, T value)
        {
#pragma warning disable CS0612 // 'ConfigurationItem'은(는) 사용되지 않습니다.
            var configItem = new ConfigurationItem(section.Name, key);
#pragma warning restore CS0612 // 'ConfigurationItem'은(는) 사용되지 않습니다.
            if (ConfigurationBase.CanSupportType(typeof(T)) == true)
            {
                config[configItem] = value;
            }
            else
            {
                config[configItem] = XmlSerializerUtility.GetString(value).Encrypt();
            }
        }
    }
}
