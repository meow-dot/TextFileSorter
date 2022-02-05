using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextFileSorter;
using TextFileSorter.Internal;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int fileSize =  1024 * 1024 * 1024;
            Directory.CreateDirectory("D:\\sorting\\");
            var fileName = string.Format($"D:\\sorting\\{fileSize}.txt");

            var get = new TextFileGenerationUtil.TextFileGenerationUtil(fileName);
            var f = await get.GenerateTestFile(fileSize);
            
            var t60 = DateTime.Now;
            var sorter6 = new InternalMergeTextFileSorter(new InternalMergeTextFileSorterOptions()
            {
                Comparer = new FileLineComparer(),
                FileStorage = "D:\\sorting",
            });
            await using var unsortedFileStream = File.OpenRead(fileName);
            await using var resultFileStream = File.Create($"D:\\sorting\\result_{DateTime.Now.Ticks}.txt");
            await sorter6.SortTextFile(unsortedFileStream, resultFileStream);
            var t61 = DateTime.Now;
            var d6 = t61 - t60;
            Console.WriteLine(d6);
            
            // var t60 = DateTime.Now;
            // var sorter6 = new MergeTextFileSorter(new MergeTextFileSorterOptions()
            // {
            //     Comparer = new FileLineComparer(),
            //     FileStorage = "D:\\sorting",
            //     BufferSize = 64 * 1024,
            //     MergeChunkSize = 10,
            //     FileSize = fileSize / 5
            // });
            // await using var unsortedFileStream = File.OpenRead(fileName);
            // await using var resultFileStream = File.Create($"D:\\sorting\\result_{DateTime.Now.Ticks}.txt");
            // await sorter6.SortTextFile(unsortedFileStream, resultFileStream);
            // var t61 = DateTime.Now;
            // var d6 = t61 - t60;
            // Console.WriteLine(d6);

            Console.WriteLine(1);
        }
    }
}
