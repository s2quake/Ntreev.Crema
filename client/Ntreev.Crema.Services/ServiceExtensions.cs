////Released under the MIT License.
////
////Copyright (c) 2018 Ntreev Soft co., Ltd.
////
////Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
////documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
////rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
////persons to whom the Software is furnished to do so, subject to the following conditions:
////
////The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
////Software.
////
////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
////WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
////COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
////OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//using Ntreev.Crema.ServiceModel;
//using Ntreev.Crema.Services.CremaHostService;
//using Ntreev.Crema.Services.DataBaseContextService;
//using Ntreev.Crema.Services.DataBaseService;
//using Ntreev.Crema.Services.DomainContextService;
//using Ntreev.Crema.Services.UserContextService;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Text;
//using System.Threading.Tasks;

//namespace Ntreev.Crema.Services
//{
//    static class ServiceExtensions
//    {
//        public static void CloseService<TChannel>(this ClientBase<TChannel> service, CloseReason reason) where TChannel : class
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                if (Environment.OSVersion.Platform == PlatformID.Unix)
//                {
//                    service.Abort();
//                }
//                else
//                {
//                    service.Close();
//                }
//            }
//            else
//            {
//                service.Abort();
//            }
//        }

//        public static void Unsubscribe(this CremaHostServiceClient service, CloseReason reason)
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                try
//                {
//                    service.Unsubscribe();
//                }
//                catch
//                {

//                }
//            }
//        }

//        public static void Unsubscribe(this DomainContextServiceClient service, CloseReason reason)
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                try
//                {
//                    service.Unsubscribe();
//                }
//                catch
//                {

//                }
//            }
//        }

//        public static void Unsubscribe(this DataBaseContextServiceClient service, CloseReason reason)
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                try
//                {
//                    service.Unsubscribe();
//                }
//                catch
//                {

//                }
//            }
//        }

//        public static void Open(this ICommunicationObject service, Func<CloseInfo, Task> closeAction)
//        {
//            service.Open();
//            service.Faulted += async (s, e) =>
//            {
//                await closeAction(new CloseInfo(CloseReason.Faulted, string.Empty));
//            };
//        }

//        public static void Unsubscribe(this DataBaseServiceClient service, CloseReason reason)
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                try
//                {
//                    service.Unsubscribe();
//                }
//                catch
//                {

//                }
//            }
//        }

//        public static void Unsubscribe(this UserContextServiceClient service, CloseReason reason)
//        {
//            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
//            {
//                try
//                {
//                    service.Unsubscribe();
//                }
//                catch
//                {

//                }
//            }
//        }
//    }
//}
