using System;
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

        public static int IndexOf<T>(this T[] list, T search)
        {
            return Array.IndexOf(list, search);
        }

        public static string[] Names(this Enum e)
        {
            return Enum.GetNames(e.GetType());
        }
    }
}