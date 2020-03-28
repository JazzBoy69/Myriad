using System;
using System.Text;

namespace Myriad.Parser
{
    public struct HTMLTags
    {
        internal const string StartSection = "<section>";
        internal const string StartMainHeader = "<h2>";
        internal const string StartHeader = "<h3>";
        internal const string StartSectionWithClass = "<section class='";
        internal const string StartDivWithClass = "<div class='";
        internal const string StartParagraphWithClass = "<p class='";
        internal const string StartDivWithID = "<div id='";
        internal const string StartParagraph = "<p>";
        internal const string StartSpan = "<span>";
        internal const string StartSpanWithClass = "<span class='";
        internal const string StartBold = "<b>";
        internal const string StartItalic = "<i>";
        internal const string StartSuper = "<sup>";
        internal const string StartAnchor = "<a";
        internal const string StartAnchorWithClass = "<a class='";
        internal const string Class = " class='";
        internal const string HREF = " HREF=";
        internal const string StartFigureWithClass = "<figure class='";
        internal const string StartImg = "<img src='";
        internal const string Width = " width='";
        internal const string EndSection = "</section>";
        internal const string EndSpan = "</span>";
        internal const string EndMainHeader = "</h2>";
        internal const string EndHeader = "</h3>";
        internal const string EndBold = "</b>";
        internal const string EndItalic = "</i>";
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
        internal const string dataStart = "datastart";
        internal const string dataEnd = "dataend";
        internal const string NonbreakingSpace = "&nbsp;";
    }

    public static class HTMLClasses
    {
        internal const string scriptureSection = "scripture-section";
        internal const string scriptureText = "scripture-text";
        internal const string scriptureQuote = "scripture-quote";
        internal const string scriptureComment = "scripture-comment";
        internal const string hidden = "hidden";
        internal const string active = "active";
        internal const string rangeData = "rangedata";
        internal const string poetic1 = "firstpoetic";
        internal const string poetic2 = "poetic";
        public const string comments = "comments";
        public const string hiddendetail = "hiddendetail";
        public const string link = "link";
        public const string sidenote = "sidenote";
    }

    public interface HTMLResponse
    {
        public void StartSpanWithClass(string className);
        public void StartAnchorWithClass(string className);
        public void StartDivWithClass(string className);

        public void StartDivWithID(string id);
        public void StartFigure(string className);
        public void StartIMG(string path);
        public void Append(char c);

        public void Append(ReadOnlySpan<char> span);
        public void Append(string stringToAppend);
        public void Append(int number);
        public void AppendHREF(string pageName);
        public void AppendIMGWidth(string widthString);
        public void AppendClass(string className);
        public string Response();
        void StartParagraphWithClass(string className);
    }
    public class HTMLStringBuilder : HTMLResponse
    {

        private StringBuilder Builder { get; } = new StringBuilder();

        public string Response()
        {
            return Builder.ToString();
        }
        public void StartSpanWithClass(string className)
        {
            Builder.Append(HTMLTags.StartSpanWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartAnchorWithClass(string className)
        {
            Builder.Append(HTMLTags.StartAnchorWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuote);
        }

        public void Append(char c)
        {
            Builder.Append(c);
        }

        public void AppendHREF(string pageName)
        {
            Builder.Append(HTMLTags.HREF);
            Builder.Append(pageName);
        }

        public void Append(string stringToAppend)
        {
            Builder.Append(stringToAppend);
        }

        public void Append(int number)
        {
            Builder.Append(number);
        }

        public void StartDivWithClass(string className)
        {
            Builder.Append(HTMLTags.StartDivWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartDivWithID(string id)
        {
            Builder.Append(HTMLTags.StartDivWithID);
            Builder.Append(id);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartFigure(string className)
        {
            Builder.Append(HTMLTags.StartFigureWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);        
        }

        public void StartIMG(string path)
        {
            Builder.Append(HTMLTags.StartImg);
            Builder.Append(path);
            Builder.Append(HTMLTags.CloseQuote);
        }

        public void AppendIMGWidth(string widthString)
        {
            Builder.Append(HTMLTags.Width);
            Builder.Append(widthString);
            Builder.Append(HTMLTags.CloseQuote);
        }

        public void AppendClass(string className)
        {
            Builder.Append(HTMLTags.Class);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuote);
        }

        public void Append(ReadOnlySpan<char> span)
        {
            Builder.Append(span);
        }

        public void StartParagraphWithClass(string className)
        {
            Builder.Append(HTMLTags.StartParagraphWithClass);
            Builder.Append(className);
            Builder.Append(HTMLTags.CloseQuoteEndTag);
        }
    }
}