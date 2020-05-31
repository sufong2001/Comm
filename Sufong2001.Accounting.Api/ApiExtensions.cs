using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sufong2001.Accounting.Api
{
    public static class ApiExtensions
    {
        public static Task<string> Content(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEndAsync();
        }
    }
}
