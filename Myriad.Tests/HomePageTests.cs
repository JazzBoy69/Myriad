using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Myriad.Library;
using Myriad.Parser;

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
            IndexPage indexPage = new IndexPage(DefaultResponse());
            paragraphs = indexPage.GetPageParagraphs();
            Assert.That(paragraphs.Count > Numbers.nothing);
        }

    }
}
