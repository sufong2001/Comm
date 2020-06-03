using Microsoft.Extensions.FileProviders;
using Sufong2001.Share.Json;
using System.IO;
using System.Threading.Tasks;

namespace Sufong2001.Share.IO
{
    public static class IoExtensions
    {
        private static readonly IFileProvider FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());

        public static Stream CreateStream(this string filePath)
        {
            return new MemoryStream(File.ReadAllBytes(FileProvider.GetFileInfo(filePath).PhysicalPath));
        }

        public static T ReadTo<T>(this string filePath)
        {
            return File.ReadAllText(FileProvider.GetFileInfo(filePath).PhysicalPath).To<T>();
        }

        public static Task<string> Content(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEndAsync();
        }
    }
}