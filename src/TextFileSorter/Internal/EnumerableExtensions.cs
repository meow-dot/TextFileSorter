using System.Collections.Generic;
using System.Linq;

namespace TextFileSorter.Internal
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Splits the elements of a sequence into chunks of size at most <paramref name="size">size</paramref>.
        /// </summary>
        /// <param name="source">An IEnumerable of T whose elements to chunk.</param>
        /// <param name="size">The maximum size of each chunk.</param>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int size) 
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / size)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}