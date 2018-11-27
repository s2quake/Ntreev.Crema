using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public struct RowItemByKey
    {
        public bool IsEnabled { get; set; }

        public TagInfo Tags { get; set; }

        public string ID { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return this.Value ?? string.Empty;
        }

        public readonly static RowItemByKey Empty = new RowItemByKey()
        {
            ID = null,
            Value = null,
        };
    }
}
