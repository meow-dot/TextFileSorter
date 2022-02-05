using System.Collections.Generic;
using System.IO;
using TextFileSorter.Internal;

namespace TextFileSorter
{
    public class MergeTextFileSorterOptions
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

        public int FileSize
        {
            get => _fileSize ?? 1024 * 1024;
            init => _fileSize = value;
        }

        public int BufferSize
        {
            get => _bufferSize ?? 64 * 1024;
            init => _bufferSize = value;
        }

        public int MergeChunkSize
        {
            get => _mergeChunkSize ?? 10;
            init => _mergeChunkSize = value;
        }
        
        private readonly IComparer<string> _comparer;
        private readonly string _fileStorage;
        private readonly int? _bufferSize;
        private readonly int? _fileSize;
        private readonly int? _mergeChunkSize;
    }
}