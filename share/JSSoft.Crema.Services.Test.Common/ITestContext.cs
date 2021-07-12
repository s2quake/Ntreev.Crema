using JSSoft.Crema.Services;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Common.Extensions;

namespace JSSoft.Crema.Services.Test.Common
{
    public interface ITestContext
    {
        Task<Authentication> LoginRandomAsync(Authority authority);
    }
}
