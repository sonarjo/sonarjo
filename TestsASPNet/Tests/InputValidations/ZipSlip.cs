// https://rules.sonarsource.com/csharp/tag/injection/RSPEC-6096

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ZipSlip
{
    public class ZipSlipNoncompliant
    {
        public void ExtractEntry(IEnumerator<ZipArchiveEntry> entriesEnumerator, string destinationDirectory)
        {
            var entry = entriesEnumerator.Current;
            var destinationPath = Path.Combine(destinationDirectory, entry.FullName);
            entry.ExtractToFile(destinationPath); // Noncompliant
        }
    }
}