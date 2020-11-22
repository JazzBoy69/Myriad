using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using NUnit.Framework;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Parser;
using Myriad.Library;
using Myriad.CitationHandlers;

namespace Myriad.Tests
{
    class CitationHandlerTests
    {
        Dictionary<string, string> Citations = new Dictionary<string, string>()
        {
            { "Mt 24:14", "Mt&nbsp;24:14" },
            { "Mt 24:45-47", "Mt&nbsp;24:45-47" }
        };
        [Test]
        public async Task CitationTests()
        {
            foreach (KeyValuePair<string, string> entry in Citations)
            {
                List<Citation> citations = CitationConverter.FromString(entry.Key);
                var writer = Writer.New();
                await CitationConverter.ToString(citations, writer);
                string citationText = writer.Response();
                Assert.AreEqual(entry.Value, citationText);
            }
        }
    }
}
