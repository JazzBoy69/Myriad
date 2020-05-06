using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using NUnit.Framework;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Parser;
using Myriad.Pages;
using Myriad.Data;
using System;

namespace Myriad.Tests
{
    [TestFixture]
    public class HomePageTests
    {
        readonly string directory = "C:\\Users\\joela\\Documents\\Test\\Myriad\\";
        readonly string name = "Index";
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
            Assert.That(paragraphs.Count > Number.nothing);
        }
        
        public async Task ParseHomePage()
        {
            ReadMarkupParagraphs();

            IndexPage page = new IndexPage();

            var writer = Writer.New();
            await page.RenderBody(writer);
            string correctResult = "";
            using (StreamReader fs = new StreamReader(directory + name + "HTML.txt"))
            {
                correctResult = fs.ReadLine();
            }
            Assert.AreEqual(correctResult, writer.Response());
        }

        private void ReadMarkupParagraphs()
        {
            paragraphs = new List<string>();
            using (StreamReader fs = new StreamReader(directory + name + "Markup.txt"))
            {

                string line;
                while ((line = fs.ReadLine()) != null)
                {

                    paragraphs.Add(line);
                }
            }
        }

        [Test]
        public async Task ReadEditParagraph()
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationParagraph),
                53, 7);
            string paragraph = await reader.GetDatum<string>();
            reader.Close();
            ReadMarkupParagraphs();
            Assert.AreEqual(paragraphs[Ordinals.second], paragraph);
        }

    }
}