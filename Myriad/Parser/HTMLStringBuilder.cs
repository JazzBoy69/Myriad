using System;
using System.Text;

namespace Myriad.Parser
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

        internal void StartSpan()
        {
            Builder.Append("<span>");
        }

        internal void StartSpan(string className)
        {
            Builder.Append("<span class='");
            Builder.Append(className);
            Builder.Append("'>");
        }

        internal void StartAnchor(string className)
        {
            Builder.Append("<a class='");
            Builder.Append(className);
            Builder.Append("'");
        }
        internal void StartAnchor()
        {
            Builder.Append("<a");
        }

        internal void StartQuery()
        {
            Builder.Append("?");
        }

        internal void Append(char c)
        {
            Builder.Append(c);
        }

        internal void AppendAmpersand()
        {
            Builder.Append("&");
        }

        internal void AppendHREF(string pageName)
        {
            Builder.Append(" HREF=");
            Builder.Append(pageName);
        }

        internal void Append(string stringToAppend)
        {
            Builder.Append(stringToAppend);
        }

        internal void EndAnchor()
        {
            Builder.Append("</a>");
        }

        internal void EndHTMLTag()
        {
            Builder.Append(">");
        }

        internal void Append(int number)
        {
            Builder.Append(number);
        }

        internal void EndDiv()
        {
            Builder.Append("</div>");
        }

        internal void StartDivWithClass(string className)
        {
            Builder.Append("<div class='");
            Builder.Append(className);
            Builder.Append("'>");
        }

        internal void StartHeader()
        {
            Builder.Append("<h3>");
        }

        internal void EndHeader()
        {
            Builder.Append("</h3>");
        }

        internal void StartFigure(string className)
        {
            Builder.Append("<figure class='");
            Builder.Append(className);
            Builder.Append("'>");        }

        internal void StartIMG(string path)
        {
            Builder.Append("<img src='");
            Builder.Append(path);
            Builder.Append("'");
        }

        internal void AppendIMGWidth(string widthString)
        {
            Builder.Append(" width='");
            Builder.Append(widthString);
            Builder.Append("'");
        }

        internal void StartSuper()
        {
            Builder.Append("<sup>");
        }

        internal void EndSuper()
        {
            Builder.Append("</sup>");
        }

        internal void StartBold()
        {
            Builder.Append("<b>");
        }

        internal void EndBold()
        {
            Builder.Append("</b>");
        }

        internal void AppendClass(string className)
        {
            Builder.Append(" class='");
            Builder.Append(className);
            Builder.Append("'");
        }

        internal void EndSingleTag()
        {
            Builder.Append(" />");
        }

        internal void EndFigure()
        {
            Builder.Append("</figure>");
        }
    }
}