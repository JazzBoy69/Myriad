using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Myriad.Library;

namespace Myriad.Parser
{
    internal class Formats
    {
        internal bool detail = false;
        internal bool bold = false;
        internal bool super = false;
        internal bool italic = false;
        internal bool heading = false;
        internal bool editable = true;
        internal bool figure = false;
        internal bool labelExists = false;
        internal bool hideDetails = false;
        internal bool sidenote = false;
        internal void Reset()
        {
            detail = false;
            bold = false;
            super = false;
            italic = false;
            heading = false;
            editable = true;
            figure = false;
            labelExists = false;
            hideDetails = false;
            sidenote = false;
        }
    }
    public class MarkupParser : IParser
    {
        protected int citationLevel = 0;
        protected IMarkedUpParagraph currentParagraph;
        protected StringRange mainRange = new StringRange();
        protected IMarkedUpParagraphCreator creator;
        protected bool foundEndToken;
        protected int lastDash;
        internal readonly Formats formats = new Formats();
        protected readonly PageFormatter formatter;
        readonly CitationHandler citationHandler;
        readonly StringRange labelRange = new StringRange();
        protected ParagraphInfo paragraphInfo;

        readonly List<Citation> allCitations = new List<Citation>();
        readonly List<string> tags = new List<string>();
        public IMarkedUpParagraph CurrentParagraph { get => currentParagraph; }
        public StringRange MainRange { get => mainRange; }
        public List<Citation> Citations => allCitations;
        public List<string> Tags => tags;

        public string ParsedText { get { return formatter.Result; } }

        public MarkupParser(HTMLResponse builder)
        {
            formatter = new PageFormatter(builder);
            citationHandler = new CitationHandler();
            paragraphInfo.type = ParagraphType.Undefined;
        }
        public void SetParagraphCreator(IMarkedUpParagraphCreator creator)
        {
            this.creator = creator;
        }

        public void SetParagraphInfo(ParagraphType type, int ID)
        {
            paragraphInfo.type = type;
            paragraphInfo.ID = ID;
        }
        virtual protected void HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            AddHTMLBeforeParagraph();
            if (currentParagraph.Length > 1)
            {
                foundEndToken = HandleStartToken();
            }
        }
        protected void Initialize()
        {
            if (currentParagraph.Length == Number.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.SetLimit(currentParagraph.Length - 1);
            lastDash = GetLastDash();
        }
        public void ParseParagraph(string paragraph, int index)
        {
            paragraphInfo.index = index;
            ResetCrossReferences();
            currentParagraph = creator.Create(paragraph);
            ParseParagraph();
        }
        protected void ParseParagraph()
        {
            try
            {
                if (currentParagraph.Length == Number.nothing) return;
                Initialize();
                HandleStart();
                if (foundEndToken) return;
                SearchForToken();
                foundEndToken = false;
                while (mainRange.Valid)
                {
                    HandleToken();
                    if (foundEndToken) break;
                    MoveToNextToken();
                }
                if (!foundEndToken) HandleEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected void MoveToNextToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            SearchForToken();
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveToNextToken();
            }
        }

        protected void SearchForEndBracketToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            mainRange.MoveEndTo(currentParagraph.IndexOf('}', mainRange.Start - 1));
        }

        public int DecreaseCitationLevel()
        {
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
        }
        private int GetLastDash()
        {
            int p = currentParagraph.LastIndexOf('—');
            if (p == Result.notfound) return Result.notfound;
            int q = currentParagraph.IndexOf('.', p);
            int r = currentParagraph.LastIndexOf('.');
            if (q != r) return Result.notfound;
            q = currentParagraph.IndexOf(' ', p);
            if (q == Result.notfound) return Result.notfound;
            r = Bible.IndexOfBook(currentParagraph.StringAt(p + 1, q - 1));
            return (r == Result.notfound) ?
                Result.notfound :
                p;

        }



        protected void MoveIndexToNextBracketToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.brackettokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveIndexToNextBracketToken();
            }
        }
        protected void ResetCitationLevel()
        {
            citationLevel = 0;
        }
        protected void ResetCrossReferences()
        {
            allCitations.Clear();
            tags.Clear();
        }
        virtual protected void AddHTMLBeforeParagraph()
        {

        }

        protected bool HandleStartToken()
        {
            int token = currentParagraph.TokenAt(Ordinals.first);
            if (token == Tokens.headingToken)
            {
                formats.editable = false;
                formats.heading = true;
                formatter.StartHeading(); 
                mainRange.BumpStart();
                mainRange.BumpStart();
                return false;
            }

            if (token == Tokens.startSidenote)
            {
                formats.editable = false;
                formats.sidenote = true;
                if (currentParagraph.Length > 2)
                    formatter.StartSidenoteWithHeading(formats);
                else
                    formatter.StartSidenote();
                SkipLongToken();
                return false;
            }
            if (token == Tokens.picture)
            {
                formats.editable = false;
                formats.figure = true;
                formatter.AppendFigure(currentParagraph.Text, formats);
                return true;
            }
            //Todo Handle Table tokens

            return false;
        }

        virtual protected void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            if (citationLevel > 0)
            {
                mainRange.MoveStartTo(lastDash + 1);
                mainRange.PullEnd();
                HandleLastDashCitations();
                mainRange.BumpEnd();
            }
            formatter.AppendString(currentParagraph, mainRange);

            if (formats.heading)
                formatter.EndHeading();
        }

        virtual protected void AddHTMLAfterParagraph()
        {
            if (formats.heading)
                formatter.EndHeading();
        }

        public void ParseMainHeading(string paragraph)
        {
            creator.Create(paragraph);
            formatter.StartMainHeading();
            formatter.AppendString(currentParagraph, Ordinals.third, currentParagraph.Length - 3);
            formatter.EndMainHeading();
            formatter.StartComments();
        }

        public void EndComments()
        {
            formatter.EndComments();
        }

        public void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
            if ((mainRange.End == Result.notfound) || ((mainRange.Start < lastDash) && (mainRange.End > lastDash)))
            {
                mainRange.MoveEndTo(lastDash);
            }
        }
        public void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            int longToken = currentParagraph.TokenAt(mainRange.End);
            if (token == '|')
            {
                if (citationLevel > 0)
                    SetLabel();
                SkipToken();
                return;
            }
            if (citationLevel > Number.nothing)
                HandleCitations();
            AppendTextUpToToken();
            if (longToken == Tokens.detail)
            {
                formats.detail = formatter.HandleDetails(formats.detail, formats);
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.bold)
            {
                formats.bold = formatter.ToggleBold(formats.bold);
                SkipLongToken();
                return;
            }
            if (token == '^')
            {
                formats.super = formatter.ToggleSuperscription(formats.super);
                SkipToken();
                return;
            }
            if (longToken == Tokens.italic)
            {
                formats.italic = formatter.ToggleItalic(formats.italic);
                SkipLongToken();
                return;
            }
            if (token == '/')
            {
                AppendToken();
                return;
            }
            if (longToken == Tokens.endSidenote)
            {
                formatter.EndSidenote(formats);
                HandleEndToken();
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.headingToken)
            {
                formatter.EndHeading();
                HandleEndToken();
                SkipLongToken();
                return;
            }
            if ((token == '(') || (token == '[') || (token=='—'))
            {
                citationLevel++;
                AppendToken();
                return;
            }
            if (token == '{')
            {
                citationLevel++;
                SkipToken();
                return;
            }
            if (token == '~')
            {
                citationLevel++;
                formatter.Append('—');
                SkipToken();
                return;
            }
            if ((token == ')') || (token == ']'))
            {
                citationLevel = DecreaseCitationLevel();
                AppendToken();
                return;
            }
            if (token == '}')
            {
                citationLevel = DecreaseCitationLevel();
                SkipToken();
                return;
            }
            if (token == '_')
            {
                formatter.Append(HTMLTags.NonbreakingSpace);
                SkipToken();
                return;
            }
            if (token == '#')
            {
                if (formats.labelExists)
                {
                    SearchForEndBracketToken();
                    if (!mainRange.Valid) return;
                    ResetCitationLevel();
                    string tag = currentParagraph.StringAt(mainRange);
                    tags.Add(tag);
                    formatter.AppendTag(currentParagraph, labelRange, mainRange);
                    formats.labelExists = false;
                    labelRange.Invalidate();
                    mainRange.GoToNextStartPosition();
                    return;
                }
                StringRange inlineLabelRange = new StringRange(mainRange.End+1, mainRange.End+1);
                MoveIndexToEndOfWord();
                inlineLabelRange.MoveEndTo(mainRange.End-1);
                formatter.AppendTag(currentParagraph, inlineLabelRange, inlineLabelRange);
                mainRange.GoToNextStartPosition();
                return;
            }
            AppendToken();
        }

        internal void SetLabel()
        {
            labelRange.Copy(mainRange);
            labelRange.PullEnd();
            mainRange.GoToNextStartPosition();
            formats.labelExists = true;
        }
        private void AppendToken()
        {
            formatter.AppendString(currentParagraph, mainRange.End, mainRange.End);
            mainRange.GoToNextStartPosition();
        }

        private void SkipToken()
        {
            mainRange.GoToNextStartPosition();
        }

        private void SkipLongToken()
        {
            mainRange.BumpEnd();
            mainRange.GoToNextStartPosition();
        }

        private void AppendTextUpToToken()
        {
            formatter.AppendString(currentParagraph, mainRange.Start, mainRange.End - 1);
        }

        protected void HandleEndToken()
        {
            foundEndToken = true;
            //todo handle closing for special tokens?
            formatter.EndSection();
        }

        public void HandleLastDashCitations()
        {
            List<Citation> citations =
                citationHandler.ParseCitations(mainRange, currentParagraph);
            if (citations.Count > 0)
            {
                if (formats.labelExists)
                {
                    citations[Ordinals.first].DisplayLabel = labelRange;
                    formatter.AppendCitationWithLabel(currentParagraph, citations[Ordinals.first]);
                    mainRange.MoveStartTo(mainRange.End);
                    return;
                }
                else
                {
                    citations.Last().Label.BumpEnd();
                    formatter.AppendCitations(currentParagraph, citations);
                    mainRange.MoveStartTo(citations[Ordinals.last].Label.End+1);
                }
            }
        }
        public void HandleCitations()
        {
            var rangeToParse = new StringRange(mainRange.Start, mainRange.End - 1);
            List<Citation> citations =
                citationHandler.ParseCitations(rangeToParse, currentParagraph);
            if (citations.Count < 1) return;
            allCitations.AddRange(citations);
            if (formats.labelExists)
            {
                citations[Ordinals.first].DisplayLabel = labelRange;
                formatter.AppendCitationWithLabel(currentParagraph, citations[Ordinals.first]);
                mainRange.MoveStartTo(mainRange.End);
                return;
            }
            else
            {
                formatter.AppendCitations(currentParagraph, citations);
                mainRange.MoveStartTo(citations[Ordinals.last].Label.End + 1);
            }
        }

        protected void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) &&
                (Symbols.IsPartOfWord(currentParagraph.CharAt(mainRange.End))))
                mainRange.BumpEnd();
        }
    }

}
