using System;
using System.Linq;
using System.Reflection;

namespace Sufong2001.Share.Assembly
{
    public static class AssemblyExtensions
    {
        public static FieldInfo[] GetStaticFieldInfos(this Type type)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            return fields;
        }

        public static string[] GetStaticValues(this Type type)
        {
            var values = type.GetStaticFieldInfos()
                .Select(f => f.GetValue(null).ToString())
                .ToArray();

            return values;
        }
    }
}