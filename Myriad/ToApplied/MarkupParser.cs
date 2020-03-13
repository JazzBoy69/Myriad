using System;
using System.Collections.Generic;
using System.Text;

namespace Myriad.ToApplied

{
    public interface MarkupParser
    {
        public StringBuilder ParsedText { get; }

        public void Parse(List<string> markupParagraphs);

    }
}