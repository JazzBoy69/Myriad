using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using NUnit.Framework;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Pages;

namespace Myriad.Tests
{
    [TestFixture]
    public class HomePageTests
    {
        List<string> paragraphs;
        MarkupParser parser;
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }
        [Test]
         public void CanGetData()
        {
            IndexPage indexPage = new IndexPage();
            paragraphs = indexPage.GetPageParagraphs();
            Assert.That(paragraphs.Count > Numbers.nothing);
        }
        [Test]
        public void ParseHomePage()
        {
            string directory = "C:\\Users\\joela\\Documents\\Test\\Myriad\\";
            string name = "Index";
            paragraphs = new List<string>();
            using (StreamReader fs = new StreamReader(directory + name+"Markup.txt"))
            {

                string line;
                while ((line = fs.ReadLine()) != null)
                {

                    paragraphs.Add(line);
                }
            }
            var builder = new HTMLStringBuilder();
            var parser = new MarkupParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.Parse(paragraphs);
            string correctResult = "";
            using (StreamReader fs = new StreamReader(directory + name + "HTML.txt"))
            {
                correctResult = fs.ReadLine();
            }
            Assert.AreEqual(correctResult, builder.Response());
        }

    }
}