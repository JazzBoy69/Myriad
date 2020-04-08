using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FelicianaLibrary;
using Myriad.Pages;
using Myriad.Library;
using Myriad.CitationHandlers;


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

        //319428096&end=319428114
        //[Benchmark]
        async public Task RenderTextWithPictures()
        {
            TextPage page = new TextPage();
            page.SetResponse(DefaultResponse());
            page.SetCitation(new Citation(394496, 394522));
            await page.RenderPage();
        }

        [Benchmark]
        async public Task RenderText()
        {
            TextPage page = new TextPage();
            page.SetResponse(DefaultResponse());
            page.SetCitation(new Citation(319422720, 319422739));
            await page.RenderPage();
        }
        //[Benchmark]
        public void ParseCitation()
        {
            string textOfCitation = "({Mark 2:1|Mr 2:1!})";
            CitationHandler citationHandler = new CitationHandler();
            IParagraph paragraph = new Paragraph()
            {
                Text = textOfCitation
            };
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
        }

        //[Benchmark]
        public void GetImageFromFile()
        {
            string filename = "Ge0605.jpg";
            ImageElement image = new Library.ImageElement(filename);
            for (int i = 0; i < 5; i++)
            {
                image.GetDimensionsFromFile();
            }
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
