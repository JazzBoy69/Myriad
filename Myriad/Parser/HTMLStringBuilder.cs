﻿using System;
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
    }
}