using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Data;
using Myriad;
using Myriad.Tests;

namespace TestStub
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.RunTest();
        }
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }
        List<string> paragraphs;
        MarkupParser parser;
        private void RunTest()
        {
            HTMLStringBuilder builder = new HTMLStringBuilder();
            string text = "Close to Capernaum and the Sea of Galilee while the crowds spread out on a level place before him, Jesus apparently climbed to a higher spot on the mountain and sat down. Jewish teachers customarily **sat down,** especially for formal teaching sessions. (Lu 6:17) The people are eager to hear the teacher who is able to perform these amazing miracles. Jesus, however, delivers his sermon mainly for the benefit of **his disciples,** who are probably gathered around closest to him. The Greek word for “#disciple,” //ma·the·tesʹ,// refers to a learner, or one who is taught, and implies a personal attachment to a teacher, an attachment that shapes the disciple’s whole life.";
            MarkupParser parser = new MarkupParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.ParseParagraph(text);
            string citationText;
            List<Citation> citation = CitationConverter.FromString("Song of Solomon 2:1");
            citationText = CitationConverter.ToString(citation);
            Console.WriteLine(citationText);
        }

        private void InitializeParser()
        {
            parser = new MarkupParser(new HTMLStringBuilder());
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            paragraphs = new List<string>();
        }
    }
}
