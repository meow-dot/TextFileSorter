using System.Collections.Generic;
using System.IO;
using TextFileSorter.Internal;

namespace TextFileSorter
{
    public class InternalMergeTextFileSorterOptions
    {
        public IComparer<string> Comparer
        {
            get => _comparer ?? new FileLineComparer();
            init => _comparer = value;
        }

        public string FileStorage
        {
            get => _fileStorage ?? Directory.GetCurrentDirectory();
            init => _fileStorage = value;
        }
        
        private readonly IComparer<string> _comparer;
        private readonly string _fileStorage;
    }
}