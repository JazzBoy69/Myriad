using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Paragraph.Implementation;

namespace Myriad.Paragraph
{
    public class ParagraphReference
    {
        public static IMarkedUpParagraph New(string s)
        {
            var paragraph = new MarkedUpParagraph
            {
                Text = s
            };
            return paragraph;
        }
    }
}
