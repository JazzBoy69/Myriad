using System;
using System.Text;

namespace Myriad.AppliedClasses
{
    public class HTMLStringBuilder
    {
        internal StringBuilder Builder { get; } = new StringBuilder();

        internal void StartSection()
        {
            Builder.Append("<section>");
        }

        internal void StartParagraph()
        {
            Builder.Append("<p>");
        }

        internal void StartParagraphContent()
        {
            Builder.Append("<span class=parcontent>");
        }

        internal void EndSpan()
        {
            Builder.Append("</span>");
        }

        internal void EndParagraph()
        {
            Builder.Append("</p>");
        }
    }
}