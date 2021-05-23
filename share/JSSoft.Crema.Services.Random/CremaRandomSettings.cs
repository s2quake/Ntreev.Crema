using System;
using System.Collections.Generic;
using System.Text;

namespace JSSoft.Crema.Services.Random
{
    public static class CremaRandomSettings
    {
        public static class TableContext
        {
            public static int MinTableCount { get; set; } = 10;

            public static int MaxTableCount { get; set; } = 200;

            public static int MinTableCategoryCount { get; set; } = 1;

            public static int MaxTableCategoryCount { get; set; } = 20;

            public static int MinRowCount { get; set; } = 100;

            public static int MaxRowCount { get; set; } = 10000;
        }

        public static class TypeContext
        {
            public static int MinTypeCount { get; set; } = 1;

            public static int MaxTypeCount { get; set; } = 20;

            public static int MinTypeCategoryCount { get; set; } = 1;

            public static int MaxTypeCategoryCount { get; set; } = 20;
        }
    }
}
