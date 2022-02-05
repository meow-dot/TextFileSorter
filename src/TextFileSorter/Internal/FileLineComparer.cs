#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TextFileSorter.Internal
{
    public class FileLineComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null && y != null)
            {
                return -1;
            }

            if (y == null && x != null)
            {
                return 1;
            }

            if (x == null || y == null)
            {
                return 0;
            }
            
            var xPointIndex = x.IndexOf('.');
            var yPointIndex = y.IndexOf('.');

            var xContent = x[(xPointIndex + 1)..];
            var yContent = y[(yPointIndex + 1)..];

            var contentComparing = string.Compare(xContent, yContent, StringComparison.OrdinalIgnoreCase);
            
            if (contentComparing != 0)
            {
                return contentComparing;
            }
            
            var xIndex = x[..xPointIndex];
            var yIndex = y[..yPointIndex];

            var indexComparing = int.Parse(xIndex).CompareTo(int.Parse(yIndex));
            
            return indexComparing;
        }
    }
}