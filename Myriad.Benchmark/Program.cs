using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Threading.Tasks;
using Myriad;
using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using Myriad.Pages;


namespace Myriad.Benchmark
{
    public class Benchmarker
    {
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }
        //[Benchmark]
        public void GetHomeData()
        {
            IndexPage indexPage = new IndexPage();
            var paragraphs = indexPage.GetPageParagraphs();
        }
        [Benchmark]
        async public Task RenderIndex()
        {
            IndexPage page = new IndexPage();
            page.SetResponse(DefaultResponse());
            await page.RenderPage();
        }

        //[Benchmark]
        public void ParseCitation()
        {
            string textOfCitation = "(Mt 24:14, 16)";
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarker>();
        }
    }

}
