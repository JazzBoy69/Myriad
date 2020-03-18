using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Threading.Tasks;
using Myriad;
using Microsoft.AspNetCore.Http;


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
            IndexPage indexPage = new IndexPage(DefaultResponse());
            var paragraphs = indexPage.GetPageParagraphs();
        }
        [Benchmark]
        async public Task RenderIndex()
        {
            IndexPage page = new IndexPage(DefaultResponse());
            await page.RenderPage();
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
