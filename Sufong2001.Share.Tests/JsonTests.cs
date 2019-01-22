using Sufong2001.Share.Json;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Sufong2001.Share.Tests
{
    public class JsonTests
    {
        private readonly ITestOutputHelper output;

        public JsonTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        internal class MergedObject
        {
            public string C { get; set; }
            public string P1 { get; set; }

            public string P2 { get; set; }

            public IList<string> A { get; set; }

            public IList<(int,int)> B { get; set; }
        }

        [Fact]
        public void JsonMergeTest()
        {
            var merged = new object[]
            {
                new { O1 = "O1", P1 = "P1", C = "C1", A = new[] {"A1", "A4", "A5"}, B = new[] {(1, 2)}         },
                new { O2 = "O2", P2 = "P2", C = "C2", A = new[] {"A2", "A4", "A6"}, B = new[] {(1, 2), (2, 3)} },
                new { O3 = "O3", P3 = "P3", C = "C3", A = new[] {"A3", "A4", "A6"}, B = new[] {(1, 2), (3, 4)} },
            }.MergeTo<MergedObject>();

            Assert.Equal("C3", merged.C);
            Assert.Equal("P1", merged.P1);
            Assert.Equal("P2", merged.P2);

            output.WriteLine(merged.ToJson());
        }
    }
}