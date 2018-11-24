using Ntreev.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ApplicationHost
{
    class AppSettings
    {
        [CommandProperty]
        public string[] PluginsPath
        {
            get; set;
        }

        /// <summary>
        /// </summary>
        [CommandProperty]
        [DefaultValue("")]
        public string BasePath
        {
            get; set;
        }

        [CommandProperty]
        public int Port
        {
            get; set;
        }

        [CommandProperty]
        public bool Run
        {
            get; set;
        }

        /// <summary>
        /// light or dark
        /// </summary>
        [CommandProperty]
        [DefaultValue("")]
        public string Theme
        {
            get; set;
        }

        /// <summary>
        /// color as #ffffff
        /// </summary>
        [CommandProperty("color")]
        [DefaultValue("")]
        public string ThemeColor
        {
            get; set;
        }

        [CommandProperty]
#if DEBUG
        [DefaultValue("en-US")]
#else
        [DefaultValue("")]
#endif
        public string Culture
        {
            get; set;
        }

        [CommandPropertyArray]
        public string[] DataBases
        {
            get; set;
        }
    }
}
