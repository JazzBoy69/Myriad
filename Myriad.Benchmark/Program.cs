using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Threading;
using Myriad;
using Microsoft.AspNetCore.Http;


namespace Myriad.Benchmark
{
    public class Benchmarker
    {
        //[Benchmark]
        public void GetHomeData()
        {
            IndexModel indexModel = new IndexModel();
            var paragraphs = indexModel.GetPageParagraphs();
        }
        [Benchmark]
        public void RenderIndex()
        {
            Startup startup = new Startup();
            startup.TestPage(new DefaultHttpContext().Response);
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
