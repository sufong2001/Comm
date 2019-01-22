using System.Linq;
using Sufong2001.Share.String;

namespace Sufong2001.Comm.Dto
{
    public class ToAddress : Address
    {
        public string Company { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override string[] ToLines()
        {
            return new[] { Company, $"{FirstName} {LastName}".Trim() }
                .Concat(base.ToLines() ?? new string[0])
                .Where(StringExtensions.IsNotNullOrWhiteSpace).ToArray();
        }
    }
}