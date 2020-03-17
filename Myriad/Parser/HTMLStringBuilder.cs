using System;
using System.Text;

namespace Myriad.Parser
{
    public struct HTMLTags
    {
        internal const string StartSection = "<section>";
        internal const string StartHeader = "<h3>";
        internal const string StartDivWithClass = "<div class='";
        internal const string StartParagraph = "<p>";
        internal const string StartSpan = "<span>";
        internal const string StartSpanWithClass = "<span class='";
        internal const string StartBold = "<b>";
        internal const string StartSuper = "<sup>";
        internal const string StartAnchor = "<a";
        internal const string StartAnchorWithClass = "<a class='";
        internal const string Class = " class='";
        internal const string HREF = " HREF=";
        internal const string StartFigureWithClass = "<figure class='";
        internal const string StartImg = "<img src='";
        internal const string Width = " width='";
        internal const string EndSpan = "</span>";
        internal const string EndHeader = "</h3>";
        internal const string EndBold = "</b>";
        internal const string EndSuper = "</sup>";
        internal const string EndDiv = "</div>";
        internal const string EndFigure = "</figure>";
        internal const string EndParagraph = "</p>";
        internal const string EndAnchor = "</a>";
        internal const string CloseQuote = "'";
        internal const string EndTag = ">";
        internal const string EndSingleTag = " />";
        internal const string CloseQuoteEndTag = "'>";
        internal const string StartQuery = "?";
        internal const string Ampersand = "&";
    }
    public class HTMLStringBuilder
    {

        internal StringBuilder Builder { get; } = new StringBuilder();

        internal void StartSection()
        {
            Builder.Append(HTMLTags.StartSection);
        }

        internal void StartParagraph()
        {
            Builder.Append(HTMLTags.StartParagraph);
        }

        internal void EndSpan()
        {
            Builder.Append(HTMLTags.EndSpan);
        }

        internal void EndParagraph()
        {
            Builder.Append(HTMLTags.EndParagraph);
        }

        internal void StartSpan()
        {
            Builder.Append(HTMLTags.StartSpan);
        }

        internal void StartSpan(string className)
        {
            Builder.Append(HTMLTags.StartSpanWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }

        internal void StartAnchor(string className)
        {
            Builder.Append(HTMLTags.StartAnchorWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuote);
        }
        internal void StartAnchor()
        {
            Builder.Append(HTMLTags.StartAnchor);
        }

        internal void StartQuery()
        {
            Builder.Append(HTMLTags.StartQuery);
        }

        internal void Append(char c)
        {
            Builder.Append(c);
        }

        internal void AppendAmpersand()
        {
            Builder.Append(HTMLTags.Ampersand);
        }

        internal void AppendHREF(string pageName)
        {
            Builder.Append(HTMLTags.HREF);
            Builder.Append(pageName);
        }

        internal void Append(string stringToAppend)
        {
            Builder.Append(stringToAppend);
        }

        internal void EndAnchor()
        {
            Builder.Append(HTMLTags.EndAnchor);
        }

        internal void EndHTMLTag()
        {
            Builder.Append(HTMLTags.EndTag);
        }

        internal void Append(int number)
        {
            Builder.Append(number);
        }

        internal void EndDiv()
        {
            Builder.Append(HTMLTags.EndDiv);
        }

        internal void StartDivWithClass(string className)
        {
            Builder.Append(HTMLTags.StartDivWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }

        internal void StartHeader()
        {
            Builder.Append(HTMLTags.StartHeader);
        }

        internal void EndHeader()
        {
            Builder.Append(HTMLTags.EndHeader);
        }

        internal void StartFigure(string className)
        {
            Builder.Append(HTMLTags.StartFigureWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);        }

        internal void StartIMG(string path)
        {
            Builder.Append(HTMLTags.StartImg);
            Builder.Append(path);
            Builder.Append(HTMLTags.CloseQuote);
        }

        internal void AppendIMGWidth(string widthString)
        {
            Builder.Append(HTMLTags.Width);
            Builder.Append(widthString);
            Builder.Append(HTMLTags.CloseQuote);
        }

        internal void StartSuper()
        {
            Builder.Append(HTMLTags.StartSuper);
        }

        internal void EndSuper()
        {
            Builder.Append(HTMLTags.EndSuper);
        }

        internal void StartBold()
        {
            Builder.Append(HTMLTags.StartBold);
        }

        internal void EndBold()
        {
            Builder.Append(HTMLTags.EndBold);
        }

        internal void AppendClass(string className)
        {
            Builder.Append(HTMLTags.Class);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuote);
        }

        internal void EndSingleTag()
        {
            Builder.Append(HTMLTags.EndSingleTag);
        }

        internal void EndFigure()
        {
            Builder.Append(HTMLTags.EndFigure);
        }
    }
}