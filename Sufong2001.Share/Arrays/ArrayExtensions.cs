using System.Linq;

namespace Sufong2001.Share.Arrays
{
    public static class ArrayExtensions
    {
        public static T[] Merge<T>(this T[][] list)
        {
            return list.Where(a => a != null)
                .SelectMany(a => a)
                .ToArray();
        }
    }
}