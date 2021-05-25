using JSSoft.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace JSSoft.Crema.ServiceHosts
{
    [Export(typeof(IComponentProvider))]
    class ServiceComponentProvider : ComponentProviderBase
    {
        [ImportingConstructor]
        public ServiceComponentProvider([ImportMany] IAdaptorHostProvider[] adaptorHostProviders,
                                 [ImportMany] ISerializerProvider[] serializerProviders,
                                 [ImportMany] IDataSerializer[] dataSerializers,
                                 [ImportMany] IExceptionDescriptor[] exceptionDescriptors)
            : base(adaptorHostProviders, serializerProviders, dataSerializers, exceptionDescriptors)
        {

        }
    }
}
