using System;
using System.Collections.Generic;
using System.Text;

namespace JSSoft.Crema.Services.Random
{
    public class DataBaseSettings
    {
        public bool Transaction { get; set; }

        public TableContextSettings TableContext { get; set; } = new TableContextSettings();

        public TypeContextSettings TypeContext { get; set; } = new TypeContextSettings();

        public static DataBaseSettings Default { get; } = new DataBaseSettings();

        #region TableContextSettings

        public class TableContextSettings
        {
            public int MinTableCount { get; set; } = 10;

            public int MaxTableCount { get; set; } = 20;

            public int MinTableCategoryCount { get; set; } = 1;

            public int MaxTableCategoryCount { get; set; } = 20;

            public int MinRowCount { get; set; } = 10;

            public int MaxRowCount { get; set; } = 50;

            public int MinChildTableCount { get; set; } = 0;

            public int MaxChildTableCount { get; set; } = 10;

            public int MinDerivedTableCount { get; set; } = 0;

            public int MaxDerivedTableCount { get; set; } = 10;
        }

        #endregion TableContextSettings

        #region TypeContextSettings

        public class TypeContextSettings
        {
            public int MinTypeCount { get; set; } = 1;

            public int MaxTypeCount { get; set; } = 20;

            public int MinTypeCategoryCount { get; set; } = 1;

            public int MaxTypeCategoryCount { get; set; } = 20;
        }

        #endregion TypeContextSettings
    }
}
