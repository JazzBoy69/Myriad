using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        }


    }
}
