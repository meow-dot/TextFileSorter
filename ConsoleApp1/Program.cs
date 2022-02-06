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
            const int fileSize =  1 * 1024;
            Directory.CreateDirectory("C:\\sorting\\");
            var fileName = string.Format($"C:\\sorting\\{fileSize}.txt");

            var get = new TextFileGenerationUtil.TextFileGenerationUtil(fileName);
            var f = await get.GenerateTestFile(fileSize);
            
            var t60 = DateTime.Now;
            var sorter6 = new InternalMergeTextFileSorter2(new InternalMergeTextFileSorterOptions()
            {
                Comparer = new FileLineComparer(),
                FileStorage = "C:\\sorting",
            });
            await using var unsortedFileStream = File.OpenRead(fileName);
            await using var resultFileStream = File.OpenWrite($"C:\\sorting\\result_{DateTime.Now.Ticks}.txt");
            await sorter6.SortTextFile(unsortedFileStream, resultFileStream);
            var t61 = DateTime.Now;
            var d6 = t61 - t60;
            Console.WriteLine(d6);
            
            
            // var t6 = DateTime.Now;
            // var lines = await File.ReadAllLinesAsync(fileName);  
            // var t60 = DateTime.Now;
            // var sorter6 = new MergeTextFileSorter(new MergeTextFileSorterOptions()
            // {
            //     Comparer = new FileLineComparer(),
            //     FileStorage = "C:\\sorting",
            //     BufferSize = 64 * 1024,
            //     MergeChunkSize = 10,
            //     FileSize = fileSize / 5
            // });
            // await using var unsortedFileStream = File.OpenRead(fileName);
            // await using var resultFileStream = File.Create($"C:\\sorting\\result_{DateTime.Now.Ticks}.txt");
            // await sorter6.SortTextFile(unsortedFileStream, resultFileStream);
            // var t61 = DateTime.Now;
            // var d6 = t61 - t60;
            // Console.WriteLine(d6);

            Console.WriteLine(1);
        }
    }
}
