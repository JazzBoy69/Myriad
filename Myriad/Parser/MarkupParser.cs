using System;
using System.Collections.Generic;
using System.Text;
using Myriad.Library;
using Range = Myriad.Library.Range;


namespace Myriad.Parser

{
    public class MarkupParser
    {
        bool detail;
        bool bold;
        bool super;
        bool italic;
        bool heading;
        readonly bool hideDetails = false;

        int citationLevel = 0;
        MarkedUpParagraph currentParagraph;
        StringRange mainRange;

        static readonly char[] tokens = new char[] { '*', '^', '/', '=', '(', '[', '{', ')', ']', '}', '~', '#', '|', '_', '+' };
        static readonly char[] brackettokens = new char[] { '|', '}' };
        readonly HTMLStringBuilder builder = new HTMLStringBuilder();
        readonly PageFormatter formatter;
        readonly CitationHandler citationHandler;
        public StringBuilder ParsedText { get { return builder.Builder; } }

        public MarkupParser()
        {
            formatter = new PageFormatter(mainRange, currentParagraph, builder);
            citationHandler = new CitationHandler();
        }
        public void Parse(List<MarkedUpParagraph> paragraphs)
        {
            foreach (MarkedUpParagraph paragraph in paragraphs)
            {
                currentParagraph = paragraph;
                ParseParagraph();
            }
        }

        private void ParseParagraph()
        {
            MoveToFirstToken();
            while (mainRange.Valid)
            {
                HandleToken();
                MoveToNextToken();
            }
        }

        private void MoveToFirstToken()
        {
            if (currentParagraph.Length == Numbers.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(tokens, mainRange.Start));
        }

        internal void MoveToNextToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(tokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveToNextToken();
            }
        }
        internal void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            char charAfterToken = currentParagraph.CharAt(mainRange.End + 1);
            if ((token == '+') && (charAfterToken == '+'))
            {
                detail = HandleDetails(detail, citationLevel);
                mainRange.BumpStart();
                return;
            }

            if ((token == '*') && (charAfterToken == '*'))
            {
                bold = formatter.ToggleBold(bold, citationLevel);
                mainRange.BumpStart();
                return;
            }
            if (token == '^')
            {
                super = formatter.ToggleSuperscription(super, citationLevel);
                return;
            }
            if (token == '/')
            {
                italic = formatter.ToggleItalic(italic, citationLevel, charAfterToken);
                mainRange.BumpStart();
                return;
            }
            if ((token == ')') && (charAfterToken == ')'))
            {
                formatter.EndSidenote(citationLevel);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '(') && (charAfterToken == '('))
            {
                heading = formatter.StartSidenote(heading);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '[') && (charAfterToken == '['))
            {
                formatter.AppendFigure(currentParagraph.Text);
                return;
            }
            if (token == '=')
            {
                formatter.AppendString();
                if (charAfterToken == '=')
                {
                    heading = formatter.ToggleHeading(heading);
                    if (!heading) return;
                }
                else builder.Append('=');
                return;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel(citationLevel, token);
                return;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                citationLevel = DecreaseCitationLevel(citationLevel);
                return;
            }
            if (token == '_')
            {
                formatter.AppendString();
                return;
            }
            if (token == '|')
            {
                formatter.SetLabel(citationLevel);
                return;
            }
            if (token == '#')
            {
                if (formatter.LabelExists)
                {
                    MoveIndexToEndBracket();
                    if (!mainRange.Valid) return;
                    citationLevel = 0;
                    formatter.AppendTag();
                    return;
                }
                formatter.AppendString(citationLevel);

                MoveIndexToEndOfWord();
            }
            formatter.AppendTag();

            formatter.AppendString();
        }

        private void MoveIndexToEndBracket()
        {
                if (mainRange.Start > currentParagraph.Length)
                    return;
                mainRange.MoveEndTo(currentParagraph.IndexOf('}', mainRange.Start - 1));
        }

        private void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) && 
                (Symbols.IsPartOfWord(currentParagraph.CharAt(mainRange.End)))) 
                mainRange.BumpEnd();
            mainRange.PullEnd();
        }

        private bool HandleDetails(bool detail, int citationLevel)
        {
            bool startSpan = false;
            if (((hideDetails) && (!detail)) && (mainRange.Length > 1))
            {
                builder.StartSpan();
                startSpan = true;
            }
            formatter.AppendString(citationLevel);
            if (hideDetails)
            {
                if (detail)
                {
                    detail = false;
                    builder.EndSpan();
                    builder.EndSpan();
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        builder.EndSpan();
                    }
                    builder.StartSpan("hiddendetail");
                    builder.StartSpan();
                }
            }
            return detail;
        }



        private int IncreaseCitationLevel(int citationLevel, char token)
        {
            formatter.AppendString(citationLevel);
            citationLevel++;
            if (token == '{')
            {
                MoveIndexToNextBracketToken();
                mainRange.PullEnd();
            }
            return citationLevel;
        }
        private int DecreaseCitationLevel(int citationLevel)
        {
            formatter.AppendString(citationLevel);
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
        }
        internal void MoveIndexToNextBracketToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(brackettokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveIndexToNextBracketToken();
            }
        }

    }
}