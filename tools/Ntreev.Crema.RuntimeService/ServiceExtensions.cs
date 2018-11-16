using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.RuntimeService
{
    static class ServiceExtensions
    {
        public static void CloseService<TChannel>(this ClientBase<TChannel> service) where TChannel : class
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
    }
}
