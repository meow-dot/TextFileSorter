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
    public class MergeTextFileSorter
    {
        private readonly IComparer<string> _comparer;
        private readonly string _tempFilesLocation;
        private readonly int _fileSize;
        private readonly int _bufferSize;
        private readonly int _mergeChunkSize;

        public MergeTextFileSorter(MergeTextFileSorterOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _comparer = options.Comparer;
            _fileSize = options.FileSize;
            _bufferSize = options.BufferSize;
            _mergeChunkSize = options.MergeChunkSize;
            _tempFilesLocation = Path.Combine(options.FileStorage, $"temp_{DateTime.Now.Ticks}");
        }

        public async Task SortTextFile(Stream input, Stream output, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(_tempFilesLocation);
            var unsortedFiles = (await SplitFile(input, cancellationToken)).ToList();
            if (unsortedFiles.Count == 1)
            {
                await SortFile(File.OpenRead(unsortedFiles.First()), output);
            }

            var sortedFiles = await SortFiles(unsortedFiles);
            await MergeFiles(sortedFiles, output, cancellationToken);

            Directory.Delete(_tempFilesLocation, true);
        }

        private async Task<IEnumerable<string>> SplitFile(Stream source, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[_fileSize];
            var extraBuffer = new List<byte>();
            var fileNames = new List<string>();
            while (source.Position < source.Length)
            {
                var runBytesRead = 0;
                while (runBytesRead < _fileSize)
                {
                    var value = source.ReadByte();
                    if (value == -1)
                    {
                        break;
                    }

                    var @byte = (byte)value;
                    buffer[runBytesRead] = @byte;
                    runBytesRead++;
                }

                var extraByte = buffer[_fileSize - 1];

                while (extraByte != '\n')
                {
                    var flag = source.ReadByte();
                    if (flag == -1)
                    {
                        break;
                    }

                    extraByte = (byte)flag;
                    extraBuffer.Add(extraByte);
                }

                var fileName = Path.Combine(_tempFilesLocation, $"splitted_{fileNames.Count}");
                await using var fileStream = File.Create(Path.Combine(_tempFilesLocation, fileName));
                await fileStream.WriteAsync(buffer.AsMemory(0, runBytesRead), cancellationToken);
                if (extraBuffer.Count > 0)
                {
                    await fileStream.WriteAsync(extraBuffer.ToArray().AsMemory(0, extraBuffer.Count), cancellationToken);
                }

                fileNames.Add(fileName);
                extraBuffer.Clear();
            }

            return fileNames;
        }

        private async Task<IEnumerable<string>> SortFiles(IEnumerable<string> files)
        {
            var sortedFiles = new List<string>();
            var fileList = files.ToList();
            for (var i = 0; i < fileList.Count; i++)
            {
                var sortedFile = Path.Combine(_tempFilesLocation, $"sorted_{i}");
                await SortFile(File.OpenRead(fileList[i]), File.OpenWrite(sortedFile));
                sortedFiles.Add(sortedFile);
            }
        
            return sortedFiles;
        }
        
        private async Task SortFile(Stream input, Stream output)
        {
            var lines = new List<string>();

            using var streamReader = new StreamReader(input);
            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                lines.Add(line);
            }

            lines = lines.AsParallel()
                .OrderBy(x => x, _comparer)
                .ToList();

            await using var streamWriter = new StreamWriter(output, bufferSize: _bufferSize);

            for (var index = 0; index < lines.Count; index++)
            {
                var row = lines[index];
                await streamWriter.WriteLineAsync(row);
            }
        }

        private async Task MergeFiles(
            IEnumerable<string> sortedFiles,
            Stream output,
            CancellationToken cancellationToken)
        {
            var sortedFilesList = sortedFiles.ToList();
            switch (sortedFilesList.Count)
            {
                case 0:
                    return;
                case 1:
                {
                    await using var stream = File.OpenRead(sortedFilesList[0]);
                    await stream.CopyToAsync(output, cancellationToken);
                    return;
                }
            }

            var filesToMerge = new List<string>();

            var sortedFilesChunks = sortedFilesList.ChunkBy(_mergeChunkSize).ToList();
            for (var index = 0; index < sortedFilesChunks.Count; index++)
            {
                var sortedFilesChunk = sortedFilesChunks[index].ToList();
                if (sortedFilesChunk.Count == 1)
                {
                    filesToMerge.Add(sortedFilesChunk[0]);
                }
                else
                {
                    var tempMergedFile = Path.Combine(_tempFilesLocation, $"merged_{DateTime.Now.Ticks}");
                    await using (var fs = File.OpenWrite(tempMergedFile))
                    {
                        await Merge(sortedFilesChunk, fs, cancellationToken);
                    }

                    filesToMerge.Add(tempMergedFile);
                }
            }

            await MergeFiles(filesToMerge, output, cancellationToken);
        }

        private async Task Merge(IEnumerable<string> sortedFiles, Stream outputStream, CancellationToken cancellationToken)
        {
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: _bufferSize);
            var rows = new List<Tuple<string, StreamReader>>();

            var streamReaders = sortedFiles.Select(x =>
                {
                    var sortedFileStream = File.OpenRead(x);
                    return new StreamReader(sortedFileStream, bufferSize: _bufferSize);
                })
                .Where(x => !x.EndOfStream)
                .ToList();

            for (var index = 0; index < streamReaders.Count; index++)
            {
                var streamReader = streamReaders[index];
                var line = await streamReader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                rows.Add(new Tuple<string, StreamReader>(line, streamReader));
            }

            while (streamReaders.Any() && rows.Any())
            {
                rows.Sort((row1, row2) => _comparer.Compare(row1.Item1, row2.Item1));
                var (minRow, streamReader) = rows.First();
                await outputWriter.WriteLineAsync(minRow.AsMemory(), cancellationToken);
                rows.RemoveAt(0);

                if (streamReader.EndOfStream)
                {
                    streamReaders.Remove(streamReader);
                    streamReader.Dispose();
                    continue;
                }

                var line = await streamReader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                rows.Add(new Tuple<string, StreamReader>(line, streamReader));
            }

            for (var index = 0; index < streamReaders.Count; index++)
            {
                var streamReader = streamReaders[index];
                streamReader.Dispose();
            }
        }
    }
}