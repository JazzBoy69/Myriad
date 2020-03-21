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

        async public override Task Parse(List<string> paragraphs)
        {
            bool foundHeading = false;
            foreach (string paragraph in paragraphs)
            {
                if ((paragraph.Length > Numbers.nothing) &&
                    (paragraph[Ordinals.first] == '='))
                    foundHeading = true;
                if (!foundHeading) continue;
                currentParagraph = creator.Create(paragraph);
                await ParseParagraph();
            }
        }
    }
}
