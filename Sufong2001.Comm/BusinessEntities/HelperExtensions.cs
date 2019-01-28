using Sufong2001.Comm.Dto;
using Sufong2001.Share.String;
using System.Linq;

namespace Sufong2001.Comm.BusinessEntities
{
    public static class HelperExtensions
    {
        /// <summary>
        /// return the file name if it matches to CommunicationManifest.FileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string IsIfManifest(this string fileName)
        {
            return new[] { fileName }.IsIfManifest();
        }

        /// <summary>
        /// return the one of the file name if it matches to CommunicationManifest.FileName.
        /// The longer file name will have the highest match
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public static string IsIfManifest(this string[] fileNames)
        {
            return fileNames?.Where(f => f.IsNotNullOrEmpty())
                .OrderByDescending(f => f.Length)
                .FirstOrDefault(f => f.EndsWith(CommunicationManifest.FileName));
        }
    }
}