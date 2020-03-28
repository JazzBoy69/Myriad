using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class CommentParser : MarkupParser
    {
        public CommentParser(HTMLResponse builder) : base(builder)
        {
        }

        public override void Parse(List<string> paragraphs)
        {
            for (int index = Ordinals.second; index<paragraphs.Count; index++)      
            {
                currentParagraph = creator.Create(paragraphs[index]);
                ParseParagraph();
            }
        }

    }
}
