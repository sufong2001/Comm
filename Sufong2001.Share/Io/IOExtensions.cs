using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Sufong2001.Share.Json;

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
    }
}
