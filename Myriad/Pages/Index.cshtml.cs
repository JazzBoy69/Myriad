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
            List<string> paragraphs = GetPageParagraphs();
            var markupParagraphs = MarkedupParagraphList<MarkedUpParagraph>
                .CreateFrom(paragraphs);
            var parser = new MarkupParser<MarkedUpParagraph>();
            parser.Parse(markupParagraphs);
        }
        public List<string> GetPageParagraphs()
        {   
            return ReaderProvider.Reader()
                .GetData<string>(DataOperation.ReadNavigationPage, home); 
        }

    }
}