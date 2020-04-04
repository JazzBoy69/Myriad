﻿using System;
using System.Text;

namespace Myriad.Parser
{
    public struct JavaScriptFunctions
    {
        internal const string HandleTabClick = "HandleTabClick(this)";
        internal const string EditParagraph = "EditParagraph(this)";
    }
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
        internal const string StartList = "<ul";
        internal const string StartListItem = "<li";
        internal const string Class = " class='";
        internal const string ID = " id=";
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
        internal const string EndList = "</ul>";
        internal const string EndListItem = "</li>";
        internal const string CloseQuote = "'";
        internal const string EndTag = ">";
        internal const string EndSingleTag = " />";
        internal const string CloseQuoteEndTag = "'>";
        internal const string StartQuery = "?";
        internal const string Ampersand = "&";
        internal const string NonbreakingSpace = "&nbsp;";
        internal const string OnClick = " onclick=";
        internal const string Data_EditType = " data-edittype=";
        internal const string Data_ID = " data-id=";
        internal const string Data_Index = " data-index=";
    }

    public static class HTMLClasses
    {
        internal const string scriptureSection = "scripture-section";
        internal const string scriptureText = "scripture-text";
        internal const string scriptureQuote = "scripture-quote";
        internal const string scriptureComment = "scripture-comment";
        internal const string hidden = "hidden";
        internal const string active = "active";
        internal const string rangeData = " rangedata";
        internal const string dataStart = " data-start=";
        internal const string dataEnd = " data-end=";
        internal const string poetic1 = "firstpoetic";
        internal const string poetic2 = "poetic";
        internal const string comments = "comments";
        internal const string hiddendetail = "hiddendetail";
        internal const string link = "link";
        internal const string sidenote = "sidenote";
        internal const string tabs = "tabs";
        internal const string tabSuffix = "-tab";
        internal const string tab = "tab";
        internal const string paragraphcontent = "parcontent";
        internal const string editparagraph = "editparagraph";
        internal const string citationStart = "citationStart";
        internal const string citationEnd = "citationEnd";
    }

    public interface HTMLWriter
    {
        public void StartSpanWithClass(string className);
        public void StartAnchorWithClass(string className);
        public void StartDivWithClass(string className);

        public void StartSectionWithClass(string className);

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
    public class HTMLStringWriter : HTMLWriter
    {
        StringBuilder writer = new StringBuilder();
        private StringBuilder Writer => writer;

        public string Response()
        {
            return Writer.ToString();
        }
        public void StartSpanWithClass(string className)
        {
            writer.Append(HTMLTags.StartSpanWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartAnchorWithClass(string className)
        {
            writer.Append(HTMLTags.StartAnchorWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void Append(char c)
        {
            writer.Append(c);
        }

        public void AppendHREF(string pageName)
        {
            writer.Append(HTMLTags.HREF);
            writer.Append(pageName);
        }

        public void Append(string stringToAppend)
        {
            writer.Append(stringToAppend);
        }

        public void Append(int number)
        {
            writer.Append(number);
        }

        public void StartDivWithClass(string className)
        {
            writer.Append(HTMLTags.StartDivWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartDivWithID(string id)
        {
            writer.Append(HTMLTags.StartDivWithID);
            writer.Append(id);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartFigure(string className)
        {
            writer.Append(HTMLTags.StartFigureWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);        
        }

        public void StartIMG(string path)
        {
            writer.Append(HTMLTags.StartImg);
            writer.Append(path);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void AppendIMGWidth(string widthString)
        {
            writer.Append(HTMLTags.Width);
            writer.Append(widthString);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void AppendClass(string className)
        {
            writer.Append(HTMLTags.Class);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void Append(ReadOnlySpan<char> span)
        {
            writer.Append(span);
        }

        public void StartParagraphWithClass(string className)
        {
            writer.Append(HTMLTags.StartParagraphWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartSectionWithClass(string className)
        {
            writer.Append(HTMLTags.StartSectionWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }
    }
}