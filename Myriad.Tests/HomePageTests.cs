using System;
using System.Collections.Generic;
using NUnit.Framework;
using Myriad;
using Myriad.Library;

namespace Myriad.Tests
{
    [TestFixture]
    public class HomePageTests
    {
        [Test]
        public void CanGetData()
        {
            IndexModel indexModel = new IndexModel();
            var paragraphs = indexModel.GetPageParagraphs();
            Assert.That(paragraphs.Count > Numbers.nothing);
        }
        [Test]
        public void CanParseParagraph()
        {
            List<string> paragraphs = new List<string>();
            paragraphs.Add("You can navigate through the contents of this study project by entering a reference to a Bible verse (for example “{Mt 5:1}”) in the search box. This will take you to the page that discusses the practical application of the verse. You can also enter a reference to a Bible chapter (for example “{Mt 13}”), to read a whole Bible chapter. You can also enter a verse followed by an exclamation mark (for example “{Mr 2:1!|Mr 2:1!}”) to see information from the //Insight// volumes or the Study Bible that relates to that verse. You will also be able to see cross-references to related verses and comments about those related verses.");
            paragraphs.Add("While reading a Bible chapter you can click on a verse number to navigate down to the paragraph level. While at the paragraph level you can click on a verse number to navigate down to the verse level.");
            paragraphs.Add("==Searching for Information==");
            IndexModel indexModel = new IndexModel();
            indexModel.Parse(paragraphs);
            Assert.That(indexModel.PageBody.ToString() == "<section><p>You can navigate through the contents of this study project by entering a reference to a Bible verse (for example “<a class=link HREF=/Text?startverse=2557185&endverse=2557185>Mt 5:1</a>”) in the search box. This will take you to the page that discusses the practical application of the verse. You can also enter a reference to a Bible chapter (for example “<a HREF=/Chapter?startverse=2559233&endverse=2559290>Mt 13</a>”), to read a whole Bible chapter. You can also enter a verse followed by an exclamation mark (for example “<a HREF=/Verse?verse=2621953>Mr 2:1!</a>”) to see information from the <i>Insight</i> volumes or the Study Bible that relates to that verse. You will also be able to see cross-references to related verses and comments about those related verses.</section><div class='clear'></div><section><p>While reading a Bible chapter you can click on a verse number to navigate down to the paragraph level. While at the paragraph level you can click on a verse number to navigate down to the verse level.</section><div class='clear'></div><section><p><h3>Searching for Information</h3></section><div class='clear'></div>");
        }
    }
}
