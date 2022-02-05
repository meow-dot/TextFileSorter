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
    public class InternalMergeTextFileSorter
    {
        private readonly IComparer<string> _comparer;
        private readonly string _tempFilesLocation;

        public InternalMergeTextFileSorter(InternalMergeTextFileSorterOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _comparer = options.Comparer;
            _tempFilesLocation = Path.Combine(options.FileStorage, $"temp_{DateTime.Now.Ticks}");
        }

        public async Task SortTextFile(Stream input, Stream output, CancellationToken cancellationToken = default)
        {
            var t1 = DateTime.Now;
            // var lines = new List<string>();
            //
            // using (var streamReader = new StreamReader(input))
            // {
            //     while (!streamReader.EndOfStream)
            //     {
            //         var line = await streamReader.ReadLineAsync();
            //         if (string.IsNullOrEmpty(line)) continue;
            //         lines.Add(line);
            //     }
            // }
            byte[] buffer = null;
            await using (input)
            {
                buffer = new byte[input.Length];
                await input.ReadAsync(buffer.AsMemory(0, (int)input.Length), cancellationToken);
            }

            var queues = new Stack<Queue<byte>>();
            queues.Push(new Queue<byte>());
            for (var i = 0; i < buffer.Length; i++)
            {
                var byt = buffer[i];
                if (byt == '\n')
                {
                    queues.Push(new Queue<byte>());
                    continue;
                }
                queues.Peek().Enqueue(byt);
            }
            var t2 = DateTime.Now;
            var d = t2 - t1;

            Console.WriteLine();
        }
        
        public T[] Slice<T>(T[] source, int index, int length)
        {       
            T[] slice = new T[length];
            Array.Copy(source, index, slice, 0, length);
            return slice;
        }
        
        // private void Merge(string[] array, int lowIndex, int middleIndex, int highIndex)
        // {
        //     var left = lowIndex;
        //     var right = middleIndex + 1;
        //     var tempArray = new int[highIndex - lowIndex + 1];
        //     var index = 0;
        //
        //     while ((left <= middleIndex) && (right <= highIndex))
        //     {
        //         if (array[left] < array[right])
        //         {
        //             tempArray[index] = array[left];
        //             left++;
        //         }
        //         else
        //         {
        //             tempArray[index] = array[right];
        //             right++;
        //         }
        //
        //         index++;
        //     }
        //
        //     for (var i = left; i <= middleIndex; i++)
        //     {
        //         tempArray[index] = array[i];
        //         index++;
        //     }
        //
        //     for (var i = right; i <= highIndex; i++)
        //     {
        //         tempArray[index] = array[i];
        //         index++;
        //     }
        //
        //     for (var i = 0; i < tempArray.Length; i++)
        //     {
        //         array[lowIndex + i] = tempArray[i];
        //     }
        // }
        //
        // private string[] MergeSort(string[] array, int lowIndex, int highIndex)
        // {
        //     if (lowIndex < highIndex)
        //     {
        //         var middleIndex = (lowIndex + highIndex) / 2;
        //         MergeSort(array, lowIndex, middleIndex);
        //         MergeSort(array, middleIndex + 1, highIndex);
        //         Merge(array, lowIndex, middleIndex, highIndex);
        //     }
        //
        //     return array;
        // }
        //
        // private string[] MergeSort(string[] array)
        // {
        //     return MergeSort(array, 0, array.Length - 1);
        // }
    }
}