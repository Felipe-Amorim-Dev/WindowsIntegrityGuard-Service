using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Service.Scanners
{
    public sealed class SfcCbsLogReader
    {
        private readonly string _cbsLogPath;

        public SfcCbsLogReader()
        {
            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            _cbsLogPath = Path.Combine(windowsDirectory, "Logs", "CBS", "CBS.log");
        }

        public long GetCurrentLength()
        {
            if (!File.Exists(_cbsLogPath))
            {
                return 0;
            }

            return new FileInfo(_cbsLogPath).Length;
        }

        public async Task<string> ReadFromOffsetAsync(long offset, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_cbsLogPath))
            {
                return string.Empty;
            }

            using FileStream stream = new(_cbsLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            if (offset < 0 || offset > stream.Length)
            {
                offset = 0;
            }

            stream.Seek(offset, SeekOrigin.Begin);

            using StreamReader reader = new(stream, Encoding.UTF8, true);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}

