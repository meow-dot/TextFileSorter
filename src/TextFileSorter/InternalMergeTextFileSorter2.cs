using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextFileSorter.Internal;

namespace TextFileSorter
{
    public class InternalMergeTextFileSorter2
    {
        private readonly IComparer<string> _comparer;

        private readonly ConcurrentBag<CustomLinkedList<string>> _sorted = new();
        private int _runningTasks;

        public InternalMergeTextFileSorter2(InternalMergeTextFileSorterOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _comparer = options.Comparer;
        }

        public async Task SortTextFile(Stream input, Stream output, CancellationToken cancellationToken = default)
        {
            var t1 = DateTime.Now;
            const int bufferSize = 4 * 1024;
            using var streamReader = new StreamReader(input, bufferSize: bufferSize);
            var lines = new List<string>(100_000);
            var portions = 0;
            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();
                if (lines.Count >= 100_000)
                {
                    var temp = new List<string>(lines);
                    var t = new Thread(() =>
                    {
                        SortList(temp);
                    });
                    t.Start();
                    _runningTasks++;
                    Console.WriteLine();
                    lines.Clear();
                    portions++;
                }
                lines.Add(line);
            }
            var t2 = DateTime.Now;


            while (_runningTasks > 0)
            {
                Thread.Sleep(500);
            }
            var d = t2 - t1;
            
            await Merge(output, cancellationToken);
            
            Console.WriteLine();
        }

        private Task SortList(IEnumerable<string> list)
        {
            var result = list.AsParallel().OrderBy(x => x, _comparer).ToList();
            var linkedList = new CustomLinkedList<string>();
            for (var index = 0; index < result.Count; index++)
            {
                var str = result[index];
                linkedList.AddFirst(str);
            }
            _sorted.Add(linkedList);
            _runningTasks--;
            return Task.CompletedTask;
        }

        private async Task Merge(Stream outputStream, CancellationToken cancellationToken)
        {
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: 4 * 1024);
            var lines = new List<Tuple<string, CustomLinkedList<string>>>();

            foreach (var linkedList in _sorted)
            {
                lines.Add(new Tuple<string, CustomLinkedList<string>>(linkedList.First(), linkedList));
            }

            while (lines.Any())
            {
                lines.Sort((row1, row2) => _comparer.Compare(row1.Item1, row2.Item1));
                var (minRow, linkedList) = lines.First();
                await outputWriter.WriteLineAsync(minRow.AsMemory(), cancellationToken);

                if (!linkedList.Any())
                {
                    continue;
                }
                linkedList.RemoveFirst();
                lines[0] = new Tuple<string, CustomLinkedList<string>>(linkedList.First(), linkedList);
            }
        }
    }
}