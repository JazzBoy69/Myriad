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
    public class IndexModel : PageModel
    {
        const string home = "home";
        public StringBuilder PageBody;

        public void OnGet()
        {
            RenderPage();
        }

        public void RenderPage()
        {
            var paragraphs = GetPageParagraphs();
            var parser = new MarkupParser();
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.Parse(paragraphs);
            PageBody = parser.ParsedText;
        }
        public List<string> GetPageParagraphs()
        {
            return TestParagraphs();
            return ReaderProvider.Reader()
                .GetData<string>(DataOperation.ReadNavigationPage, home); 
        }

        private List<string> TestParagraphs()
        {
            var results = new List<string>();
            results.Add("**bold**");
            return results;
        }

    }
}