using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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

        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}