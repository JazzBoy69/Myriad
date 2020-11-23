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
            { "Second Corinthians 6:14-7:1", "Second Corinthians&nbsp;6:14–7:1" },
            { "Ge 6-8", "Ge&nbsp;6-8" },
            { "Ge 6, 7", "Ge&nbsp;6,&nbsp;7" },
            { "Re 16:14-16, 18", "Re&nbsp;16:14-16, 18" },
            { "Re 16:14, 16", "Re&nbsp;16:14, 16" },
            { "Re 16:14, 16-18", "Re&nbsp;16:14, 16-18" },
            { "Mt 2:12, 22", "Mt&nbsp;2:12, 22" },
            { "Mr 6:1, 4-6", "Mr&nbsp;6:1, 4-6" },
            { "Jas 1:13, 17", "Jas&nbsp;1:13, 17" },
            { "Jude 18-21, 25", "Jude&nbsp;18-21, 25" },
            {"1 John 5:3", "1&nbsp;John&nbsp;5:3" },
            {"First John 5:3", "First John&nbsp;5:3" },
            {"Song of Solomon 2:1", "Song&nbsp;of Solomon&nbsp;2:1" },
            {"2Jo 10, 12-14", "2Jo&nbsp;10, 12-14" },
            {"2 John 10, 12-14", "2&nbsp;John&nbsp;10, 12-14" },
            {"Second John 10, 12-14", "Second John&nbsp;10, 12-14" },
            {"2Jo 10, 12", "2Jo&nbsp;10, 12" },
            {"Mt 28:19, 20", "Mt&nbsp;28:19,&nbsp;20" },
            {"3Jo 1, 5-8", "3Jo&nbsp;1, 5-8" },
            {"3 John 1, 5-8", "3&nbsp;John&nbsp;1, 5-8" },
            {"Third John 1, 5-8", "Third John&nbsp;1, 5-8" },
            {"Ps 94, 95, 98", "Ps&nbsp;94,&nbsp;95, 98" },
            {"Psalms 94, 95, 98", "Psalms&nbsp;94,&nbsp;95, 98" },
            {"Psalm 94", "Psalm&nbsp;94" },
            {"Ge 36:2, 5-8, 14", "Ge&nbsp;36:2, 5-8, 14" },
            {"Mt 5:3, 10, 19, 20", "Mt&nbsp;5:3, 10, 19,&nbsp;20" },
            {"Joh 8:26, 28, 38, 42", "Joh&nbsp;8:26, 28, 38, 42" },
            {"Joh 8:26, 28, 38-42", "Joh&nbsp;8:26, 28, 38-42" },
            {"Mr 2:1!", "Mr&nbsp;2:1" },
            {"Mt 24:14.8", "Mt&nbsp;24:14" },
            { "Mt 24:14.preached", "Mt&nbsp;24:14" }
        };
        Dictionary<string, string> NewCitations = new Dictionary<string, string>()
        {
            
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
