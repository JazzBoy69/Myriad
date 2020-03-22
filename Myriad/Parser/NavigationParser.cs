using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class NavigationParser :MarkupParser
    {
        public NavigationParser(HTMLResponse builder) : base(builder)
        {

        }

        public override void Parse(List<string> paragraphs)
        {
            bool foundFirstHeading = false;
            foreach (string paragraph in paragraphs)
            {
                if ((paragraph.Length > Numbers.nothing) &&
                    (paragraph[Ordinals.first] == '='))
                    foundFirstHeading = true;
                if (!foundFirstHeading) continue;
                currentParagraph = creator.Create(paragraph);
                ParseParagraph();
            }
        }
    }
}
