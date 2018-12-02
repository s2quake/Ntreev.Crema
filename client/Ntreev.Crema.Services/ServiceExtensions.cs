using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.CremaHostService;
using Ntreev.Crema.Services.DataBaseContextService;
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.DomainContextService;
using Ntreev.Crema.Services.UserContextService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    static class ServiceExtensions
    {
        public static void CloseService<TChannel>(this ClientBase<TChannel> service, CloseReason reason) where TChannel : class
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    service.Abort();
                }
                else
                {
                    service.Close();
                }
            }
            else
            {
                service.Abort();
            }
        }

        public static void Unsubscribe(this CremaHostServiceClient service, CloseReason reason)
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                try
                {
                    service.Unsubscribe();
                }
                catch
                {

                }
            }
        }

        public static void Unsubscribe(this DomainContextServiceClient service, CloseReason reason)
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                try
                {
                    service.Unsubscribe();
                }
                catch
                {

                }
            }
        }

        public static void Unsubscribe(this DataBaseContextServiceClient service, CloseReason reason)
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                try
                {
                    service.Unsubscribe();
                }
                catch
                {

                }
            }
        }

        public static void Open(this ICommunicationObject service, Func<CloseInfo, Task> closeAction)
        {
            service.Open();
            service.Faulted += async (s, e) =>
            {
                await closeAction(new CloseInfo(CloseReason.Faulted, string.Empty));
            };
        }

        public static void Unsubscribe(this DataBaseServiceClient service, CloseReason reason)
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                try
                {
                    service.Unsubscribe();
                }
                catch
                {

                }
            }
        }

        public static void Unsubscribe(this UserContextServiceClient service, CloseReason reason)
        {
            if (reason != CloseReason.Faulted && reason != CloseReason.NoResponding)
            {
                try
                {
                    service.Unsubscribe();
                }
                catch
                {

                }
            }
        }
    }
}
