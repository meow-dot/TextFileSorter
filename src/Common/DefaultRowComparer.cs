#nullable enable
using System;
using System.Collections.Generic;

namespace Common
{
    public class DefaultFileRowComparer : IComparer<FileRow>
    {
        public int Compare(FileRow? x, FileRow? y)
        {
            if(x == null) throw new ArgumentNullException(nameof(x));
            if(y == null) throw new ArgumentNullException(nameof(y));
            
            int primary = string.Compare(x.Content, y.Content, StringComparison.InvariantCulture);
            if (primary != 0)
            {
                return primary;
            }

            return x.Index.CompareTo(y.Index);
        }
    }
}