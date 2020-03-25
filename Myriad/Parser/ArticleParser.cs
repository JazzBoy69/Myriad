using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class ArticleParser : MarkupParser
    {
        public ArticleParser(HTMLResponse builder) : base(builder)
        {

        }

        public override void Parse(List<string> paragraphs)
        {
            bool foundFirstHeading = false;
            foreach (string paragraph in paragraphs)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraph.Length > Numbers.nothing) &&
                        (paragraph[Ordinals.first] == '='))
                    {
                        currentParagraph = creator.Create(paragraph);
                        ParseMainHeading();
                        foundFirstHeading = true;
                    }
                    continue;
                }
                currentParagraph = creator.Create(paragraph);
                ParseParagraph();
            }
            EndComments();
        }
    }
}

