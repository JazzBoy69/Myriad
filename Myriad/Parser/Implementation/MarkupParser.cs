using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FelicianaLibrary;
using FelicianaHTML;
using Myriad.Library;
using Myriad.CitationHandlers;
using Myriad.Parser.Helpers;

namespace Myriad.Parser
{
    //todo move to parser library; use general application to test
    public class MarkupParser : IParser
    {
        protected string startHTML;
        protected string endHTML;
        protected int citationLevel = 0;
        protected IParagraph currentParagraph;
        protected StringRange mainRange = new StringRange();
        protected bool foundEndToken;
        protected int lastDash;
        internal readonly Formats formats = new Formats();
        protected readonly PageFormatter formatter;
        readonly CitationHandler citationHandler;
        readonly StringRange labelRange = new StringRange();
        protected ParagraphInfo paragraphInfo;

        readonly List<Citation> allCitations = new List<Citation>();
        readonly List<string> tags = new List<string>();
        public IParagraph CurrentParagraph { get => currentParagraph; }
        public StringRange MainRange { get => mainRange; }
        public List<Citation> Citations => allCitations;
        public List<string> Tags => tags;

        public string ParsedText { get { return formatter.Result; } }

        public MarkupParser(HTMLWriter builder)
        {
            formatter = new PageFormatter(builder);
            citationHandler = new CitationHandler();
            paragraphInfo.type = ParagraphType.Undefined;
        }

        public void SetParagraphInfo(ParagraphType type, int ID)
        {
            paragraphInfo.type = type;
            paragraphInfo.ID = ID;
        }
        virtual protected async Task HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            await AddHTMLBeforeParagraph();
            if (currentParagraph.Length > 1)
            {
                foundEndToken = await HandleStartToken();
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
        public async Task ParseParagraph(string paragraph, int index)
        {
            paragraphInfo.index = index;
            ResetCrossReferences();
            currentParagraph = new Paragraph()
            {
                Text = paragraph
            };
            await ParseParagraph();
        }
        protected async Task ParseParagraph()
        {
            try
            {
                if (currentParagraph.Length == Number.nothing) return;
                Initialize();
                await HandleStart();
                if (foundEndToken) return;
                SearchForToken();
                foundEndToken = false;
                while (mainRange.Valid)
                {
                    await HandleToken();
                    if (foundEndToken) break;
                    MoveToNextToken();
                }
                if (!foundEndToken) await HandleEnd();
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
        virtual protected async Task AddHTMLBeforeParagraph()
        {
            if (startHTML == null) return;
            await formatter.Append(startHTML);
        }

        protected async Task<bool> HandleStartToken()
        {
            int token = currentParagraph.TokenAt(Ordinals.first);
            if (token == Tokens.headingToken)
            {
                formats.editable = false;
                formats.heading = true;
                await formatter.Append(HTMLTags.StartHeader);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return false;
            }

            if (token == Tokens.startSidenote)
            {
                formats.editable = false;
                formats.sidenote = true;
                if (currentParagraph.Length > 2)
                    await formatter.StartSidenoteWithHeading(formats);
                else
                    await formatter.StartSidenote();
                SkipLongToken();
                return false;
            }
            if (token == Tokens.picture)
            {
                formats.editable = false;
                formats.figure = true;
                await formatter.AppendFigure(currentParagraph.Text, formats);
                return true;
            }
            //Todo Handle Table tokens

            return false;
        }

        virtual protected async Task HandleEnd()
        {
            mainRange.MoveEndToLimit();
            if (citationLevel > 0)
            {
                mainRange.MoveStartTo(lastDash + 1);
                mainRange.PullEnd();
                await HandleLastDashCitations();
                mainRange.BumpEnd();
            }
            await formatter.AppendString(currentParagraph, mainRange);
            await AddHTMLAfterParagraph();
        }

        virtual protected async Task AddHTMLAfterParagraph()
        {
            if (formats.heading)
            {
                await formatter.Append(HTMLTags.EndHeader);
            }
            if (endHTML == null) return;
            await formatter.Append(endHTML);
        }

        public async Task ParseMainHeading(string paragraph)
        {
            currentParagraph = new Paragraph()
            {
                Text = paragraph
            };

            await formatter.Append(HTMLTags.StartMainHeader);
            await formatter.AppendString(currentParagraph, Ordinals.third, currentParagraph.Length - 3);
            await formatter.Append(HTMLTags.EndMainHeader);
            await formatter.StartComments();
        }

        public void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
            if ((mainRange.End == Result.notfound) || ((mainRange.Start < lastDash) && (mainRange.End > lastDash)))
            {
                mainRange.MoveEndTo(lastDash);
            }
        }
        public async Task HandleToken()
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
                await HandleCitations();
            await AppendTextUpToToken();
            if (longToken == Tokens.detail)
            {
                formats.detail = await formatter.HandleDetails(formats.detail, formats);
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.bold)
            {
                formats.bold = await formatter.ToggleBold(formats.bold);
                SkipLongToken();
                return;
            }
            if (token == '^')
            {
                formats.super = await formatter.ToggleSuperscription(formats.super);
                SkipToken();
                return;
            }
            if (longToken == Tokens.italic)
            {
                formats.italic = await formatter.ToggleItalic(formats.italic);
                SkipLongToken();
                return;
            }
            if (token == '/')
            {
                await AppendToken();
                return;
            }
            if (longToken == Tokens.endSidenote)
            {
                await formatter.EndSidenote(formats);
                await HandleEndToken();
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.headingToken)
            {
                await formatter.Append(HTMLTags.EndHeader);
                await HandleEndToken();
                SkipLongToken();
                return;
            }
            if ((token == '(') || (token == '[') || (token=='—'))
            {
                citationLevel++;
                await AppendToken();
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
                await formatter.Append("—");
                SkipToken();
                return;
            }
            if ((token == ')') || (token == ']'))
            {
                citationLevel = DecreaseCitationLevel();
                await AppendToken();
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
                await formatter.Append(HTMLTags.NonbreakingSpace);
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
                    await formatter.AppendTag(currentParagraph, labelRange, mainRange);
                    formats.labelExists = false;
                    labelRange.Invalidate();
                    mainRange.GoToNextStartPosition();
                    return;
                }
                StringRange inlineLabelRange = new StringRange(mainRange.End+1, mainRange.End+1);
                MoveIndexToEndOfWord();
                inlineLabelRange.MoveEndTo(mainRange.End-1);
                await formatter.AppendTag(currentParagraph, inlineLabelRange, inlineLabelRange);
                mainRange.GoToNextStartPosition();
                return;
            }
            await AppendToken();
        }

        internal void SetLabel()
        {
            labelRange.Copy(mainRange);
            labelRange.PullEnd();
            mainRange.GoToNextStartPosition();
            formats.labelExists = true;
        }
        private async Task AppendToken()
        {
            await formatter.AppendString(currentParagraph, mainRange.End, mainRange.End);
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

        private async Task AppendTextUpToToken()
        {
            await formatter.AppendString(currentParagraph, mainRange.Start, mainRange.End - 1);
        }

        protected async Task HandleEndToken()
        {
            foundEndToken = true;
            //todo handle closing for special tokens?
            await formatter.Append(HTMLTags.EndSection);
        }

        public async Task HandleLastDashCitations()
        {
            List<Citation> citations =
                citationHandler.ParseCitations(mainRange, currentParagraph);
            if (citations.Count > 0)
            {
                if (formats.labelExists)
                {
                    citations[Ordinals.first].DisplayLabel = labelRange;
                    await formatter.AppendCitationWithLabel(currentParagraph, citations[Ordinals.first]);
                    mainRange.MoveStartTo(mainRange.End);
                    return;
                }
                else
                {
                    citations.Last().Label.BumpEnd();
                    await formatter.AppendCitations(currentParagraph, citations);
                    mainRange.MoveStartTo(citations[Ordinals.last].Label.End+1);
                }
            }
        }
        public async Task HandleCitations()
        {
            var rangeToParse = new StringRange(mainRange.Start, mainRange.End - 1);
            List<Citation> citations =
                citationHandler.ParseCitations(rangeToParse, currentParagraph);
            if (citations.Count < 1) return;
            allCitations.AddRange(citations);
            if (formats.labelExists)
            {
                citations[Ordinals.first].DisplayLabel = labelRange;
                await formatter.AppendCitationWithLabel(currentParagraph, citations[Ordinals.first]);
                mainRange.MoveStartTo(mainRange.End);
                return;
            }
            else
            {
                await formatter.AppendCitations(currentParagraph, citations);
                mainRange.MoveStartTo(citations[Ordinals.last].Label.End + 1);
            }
        }

        protected void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) &&
                (IsPartOfTag(currentParagraph.CharAt(mainRange.End))))
                mainRange.BumpEnd();
        }
        public static bool IsPartOfTag(char p)
        {
            if (((p >= 'A') && (p <= 'Z')) || ((p >= 'a') && (p <= 'z')) || (p == '\'') || (p == '`') || (p == '-') || (p == '_') || (p == '(') || (p == ')')) return true;
            return false;
        }
        public void SetStartHTML(string html)
        {
            startHTML = html;
        }

        public void SetEndHTML(string html)
        {
            endHTML = html;
        }
    }

}
