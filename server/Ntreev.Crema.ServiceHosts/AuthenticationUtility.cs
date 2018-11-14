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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.ServiceHosts
{
    static class AuthenticationUtility
    {
        private readonly static TimeSpan pingTimeout = new TimeSpan(0, 1, 0);
        private static Dictionary<Authentication, Description> authentications = new Dictionary<Authentication, Description>();
        private static CremaDispatcher dispatcher;

        private static Timer timer;

        public static Task<int> AddRefAsync(this Authentication authentication, ICremaServiceItem obj)
        {
            return Dispatcher.InvokeAsync(() =>
            {
                if (authentications.ContainsKey(authentication) == false)
                {
                    throw new ArgumentException(nameof(authentication));
                }

                var description = authentications[authentication];
                description.ServiceItems.Add(obj);

                return description.ServiceItems.Count;
            });
        }

        public static Task<int> AddRefAsync(this Authentication authentication, ICremaServiceItem obj, Action<Authentication> action)
        {
            return Dispatcher.InvokeAsync(() =>
            {
                if (authentications.Any() == false && timer == null)
                {
#if !DEBUG
                    timer = new Timer(30000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
#endif
                }

                if (authentications.ContainsKey(authentication) == false)
                {
                    authentications[authentication] = new Description(authentication, action);
                }

                var description = authentications[authentication];
                description.ServiceItems.Add(obj);
                return description.ServiceItems.Count;
            });
        }

        public static async Task<int> RemoveRefAsync(this Authentication authentication, ICremaServiceItem obj)
        {
            return await await dispatcher.InvokeAsync(async () =>
            {
                var description = authentications[authentication];
                description.ServiceItems.Remove(obj);

                try
                {
                    if (description.ServiceItems.Any() == false)
                    {
                        authentications.Remove(authentication);
                        await description.DisposeAsync();
                    }

                    return description.ServiceItems.Count;
                }
                finally
                {
                    if (authentications.Any() == false && timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                        timer = null;
                    }
                }
            });
        }

        public static Task PingAsync(this Authentication authentication)
        {
            return Dispatcher.InvokeAsync(() =>
            {
                if (authentications.ContainsKey(authentication) == true)
                {
                    authentications[authentication].Ping();
                }
            });
        }

        public static void Dispose()
        {
            dispatcher?.Dispose();
            dispatcher = null;
        }

        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var descriptionList = new List<Description>();
            lock (authentications)
            {
                var dateTime = DateTime.Now;
                foreach (var item in authentications.ToArray())
                {
                    var authentication = item.Key;
                    var description = item.Value;
                    if (dateTime - description.DateTime > pingTimeout)
                    {
                        descriptionList.Add(item.Value);
                        authentications.Remove(item.Key);
                    }
                }
            }

            var tasks = descriptionList.Select(item => item.DisposeAsync()).ToArray();
            await Task.WhenAll(tasks);
        }

        private static CremaDispatcher Dispatcher
        {
            get
            {
                if (dispatcher == null)
                    dispatcher = new CremaDispatcher(typeof(AuthenticationUtility));
                return dispatcher;
            }
        }

        #region classes

        class Description
        {
            private readonly Action<Authentication> action;

            public Description(Authentication authentication, Action<Authentication> action)
            {
                this.Authentication = authentication;
                this.Authentication.Expired += Authentication_Expired;
                this.action = action;
                this.DateTime = DateTime.Now;
            }

            private async void Authentication_Expired(object sender, EventArgs e)
            {
                this.Authentication.Expired -= Authentication_Expired;
                await Dispatcher.InvokeAsync(() =>
                {
                    authentications.Remove(Authentication);
                });
            }

            public void Ping()
            {
                this.DateTime = DateTime.Now;
            }

            public async Task DisposeAsync()
            {
                await Task.Delay(1);
                this.Authentication.Expired -= Authentication_Expired;
                if (this.ServiceItems.Any() == true)
                    this.action(this.Authentication);
            }

            public Authentication Authentication { get; }

            public List<ICremaServiceItem> ServiceItems { get; } = new List<ICremaServiceItem>();

            public DateTime DateTime { get; private set; }

            private async Task AbortServieItemsAsync(bool disconnect)
            {
                var items = this.ServiceItems.ToArray().Reverse();
                var tasks = items.Select(item => item.CloseAsync(disconnect)).ToArray();
                await Task.WhenAll(tasks);
            }
        }

        #endregion
    }
}
