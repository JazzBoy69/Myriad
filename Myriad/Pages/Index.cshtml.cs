using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using Myriad.Parser;
using Myriad.Data;

namespace Myriad
{
    public class IndexModel 
    {
        const string home = "home";
        public StringBuilder PageBody;

        public void OnGet()
        {
            RenderPage();
        }

        public void RenderPage()
        {
            object markupParagraphs = GetPageParagraphs();
            Parse(markupParagraphs as List<MarkedUpParagraph>);
        }
        public object GetPageParagraphs()
        {
            return DataReader.GetData(DataOperation.ReadNavigationPage, home); 
        }
        public void Parse(List<MarkedUpParagraph> markupParagraphs)
        {
            MarkupParser parser = new MarkupParser();
            parser.Parse(markupParagraphs);
            PageBody = parser.ParsedText;
        }

    }
}