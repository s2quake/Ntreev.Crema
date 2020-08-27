using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServerService.Test
{
    [TestClass]
    public class TestInitializer
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {

        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {

        }
    }
}
