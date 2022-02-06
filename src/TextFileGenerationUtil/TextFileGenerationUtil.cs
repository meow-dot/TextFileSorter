using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace TextFileGenerationUtil
{
    public class TextFileGenerationUtil
    {
        private const string TempFileNameTemplate = "C:\\sorting\\temp_file_{0}.txt";
        private const int InitialRowsPortionSize = 100_000;

        private readonly string[] _words = {
            "Apple",
            "Something something something",
            "Cherry is the best",
            "Banana is yellow",
            "Happy New Year",
            "lorem ipsum",
            "foo",
            "..."
        };

        private readonly string _fileName;

        public TextFileGenerationUtil(string fileName)
        {
            _fileName = fileName;
        }
        
        public async Task<string> GenerateTestFile(long maxFileSize, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory("C:\\sorting\\");
            var tempFileName = string.Format(TempFileNameTemplate, DateTime.Now.Ticks);

            if (File.Exists(_fileName))
            {
                return _fileName;
            }

            await using (File.Create(tempFileName))
            {
            }

            await using (File.Create(_fileName))
            {
            }
            
            var portionSize = InitialRowsPortionSize;
            
            while (portionSize >= 1)
            {
                await using (var streamWriter = File.AppendText(tempFileName))
                {
                    await streamWriter.WriteLineAsync(GetTextPortion(portionSize));
                }

                var currentTempFileSize = new FileInfo(tempFileName).Length;
                if (currentTempFileSize > maxFileSize)
                {
                    File.Delete(tempFileName);
                    File.Copy(_fileName, tempFileName);
                    portionSize /= 2;
                }
                else
                {
                    File.Delete(_fileName);
                    File.Copy(tempFileName, _fileName);
                }
            }
            
            File.Delete(tempFileName);

            return _fileName;
        }
        
        private string GetTextPortion(int portionSize)
        {
            return string.Join(
                "\r\n",
                Enumerable.Repeat(0, portionSize).Select(x => GeneratePseudoRandomFileRow().ToString()));
        }

        private FileRow GeneratePseudoRandomFileRow()
        {
            var random = new Random();
            var num = random.Next(1, 1000);

            return new FileRow()
            {
                Index = num,
                Content = _words[random.Next(_words.Length)]
            };
        }
    }
}