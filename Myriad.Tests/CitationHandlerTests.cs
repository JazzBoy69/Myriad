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
            { "Mt 24:45-47", "Mt&nbsp;24:45-47" },
            { "Mt 24:14, 16-18", "Mt&nbsp;24:14, 16-18" },
            { "Mt 24:14, 16, 18", "Mt&nbsp;24:14, 16, 18" },
            { "Mt 24:14, 15, 18", "Mt&nbsp;24:14,&nbsp;15, 18" },
            { "Mt 24:14; Mr 13:10", "Mt&nbsp;24:14; Mr&nbsp;13:10" },
            { "Mt 24:14; 28:19, 20", "Mt&nbsp;24:14; 28:19,&nbsp;20" },
            { "Mt 24", "Mt&nbsp;24" },
            { "Ge 30:13, 17-20, 22-24", "Ge&nbsp;30:13, 17-20, 22-24" },
            { "Mt 25:31-33, 40", "Mt&nbsp;25:31-33, 40" },
            { "Ge 29:32–30:13, 17-20", "Ge&nbsp;29:32–30:13, 17-20" },
            { "Joh 18:29–19:16", "Joh&nbsp;18:29–19:16" },
            { "Mt 2:4-6, 14, 15, 19-23", "Mt&nbsp;2:4-6, 14,&nbsp;15, 19-23" },
            { "Ge 36:2, 14, 18, 20, 24, 25", "Ge&nbsp;36:2, 14, 18, 20, 24,&nbsp;25" },
            { "Ge 36:2, 4, 10, 12", "Ge&nbsp;36:2, 4, 10, 12" },
            { "Mt 3:1, 6, 13-17", "Mt&nbsp;3:1, 6, 13-17" },
            { "Joh 8:26, 28, 38", "Joh&nbsp;8:26, 28, 38" },
            { "Mt 6:33; 24:45-47", "Mt&nbsp;6:33; 24:45-47" },
            { "2Co 6:14–7:1", "2Co&nbsp;6:14–7:1" },
            { "2 Corinthians 6:14–7:1", "2&nbsp;Corinthians&nbsp;6:14–7:1" },
            { "Second Corinthians 6:14-7:1", "Second Corinthians&nbsp;6:14–7:1" }
        };
        Dictionary<string, string> NewCitations = new Dictionary<string, string>()
        {
            { "Ge 6-8", "Ge&nbsp;6-8" }
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
        [Test]
        public async Task NewCitationTests()
        {
            foreach (KeyValuePair<string, string> entry in NewCitations)
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
