using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using Myriad.FromApplied;
using Myriad.ToApplied;

namespace Myriad
{
    public class IndexModel : DataParserPageModel
    {
        public StringBuilder PageBody;
        public void OnGet()
        {
            RenderPage();
        }

        public void RenderPage()
        {
            List<string> markupParagraphs = GetPageParagraphs();
            Parse(markupParagraphs);
        }
        public List<string> GetPageParagraphs()
        {
            DataReader reader = GetDataReader(NavigationPages.selector);
            return reader.GetData(NavigationPages.homepage);
        }
        public void Parse(List<string> markupParagraphs)
        {
            MarkupParser parser = GetMarkupParser();
            parser.Parse(markupParagraphs);
            PageBody = parser.ParsedText;
        }

    }
}