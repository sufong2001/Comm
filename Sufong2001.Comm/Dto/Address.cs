using System;
using System.Linq;
using Sufong2001.Share.String;

namespace Sufong2001.Comm.Dto
{
    public class Address
    {
        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Line3 { get; set; }

        public string State { get; set; }

        public string Suburb { get; set; }

        public string Postcode { get; set; }

        public string Country { get; set; }

        public virtual string[] ToLines()
        {
            return new[]
                {
                    Line1, Line2, Line3, $"{Suburb} {State} {Postcode}".Trim(), Country
                }
                .Where(StringExtensions.IsNotNullOrWhiteSpace).ToArray();
        }

        public override string ToString() => ToLines().ArrayToString(Environment.NewLine);
    }
}