using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class NavigationParser : MarkupParser
    {
        public NavigationParser(HTMLResponse builder) : base(builder)
        {

        }

        public override void Parse(List<string> paragraphs)
        {
            bool foundFirstHeading = false;
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[index].Length > Numbers.nothing) &&
                        (paragraphs[index][Ordinals.first] == '='))
                    {
                        currentParagraph = creator.Create(paragraphs[index]);
                        ParseMainHeading();
                        foundFirstHeading = true;
                    }
                    continue;
                }
                currentParagraph = creator.Create(paragraphs[index]);
                paragraphInfo.index = index;
                ParseParagraph();
            }
            EndComments();
        }
    }
}
