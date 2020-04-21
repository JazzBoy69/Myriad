using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Search;
using Feliciana.HTML;
using Feliciana.Library;

namespace Myriad.Pages
{
    public class VersePage : ScripturePage
    {
        public const string pageURL = "/Verse";

        //todo implement verse page
        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Verse;
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            await CitationConverter.ToLongString(citation, writer);
        }

        protected override string PageScripts()
        {
            return Scripts.Text;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            if (citation.CitationType == CitationTypes.Text)
            {
                TextPage textPage = new TextPage();
                textPage.SetCitation(citation);
                textPage.SetResponse(response);
                await textPage.RenderBody(writer);
                return;
            }
            await ParseRubyText(writer);
            await ParsePhraseComments(writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }

        private async Task ParseRubyText(HTMLWriter writer)
        {
            await StartRubySection(writer);
            List<Keyword> keywords = ReadKeywords(citation);
            if (keywords != null)
            {
                int lastWordOnTop = -1;
                List<RubyInfo> wordsOnTop = new List<RubyInfo>();
                for (int i = Ordinals.first; i < keywords.Count; i++)
                {
                    if (lastWordOnTop == Result.notfound)
                    {
                        wordsOnTop = GetSustituteText(keywords[i].ID);
                        if (wordsOnTop.Count > Number.nothing)
                        {
                            await writer.Append("<ruby>");
                            lastWordOnTop = wordsOnTop[Ordinals.last].EndID;
                        }
                    }
                    await TextFormatter.AppendTextOfKeyword(writer, keywords[i]);
                    if (keywords[i].ID == lastWordOnTop)
                    {
                        await writer.Append("<rt>");
                        await WriteSubstituteText(writer, wordsOnTop);
                        await writer.Append("</rt></ruby>");
                        lastWordOnTop = Result.notfound;
                    }
                }
            }
            await EndRubySection(writer);
        }

        private async Task WriteSubstituteText(HTMLWriter writer, List<RubyInfo> wordsOnTop)
        {
            for (int i = Ordinals.first; i < wordsOnTop.Count; i++)
            {
                if (i > Ordinals.first)
                    await writer.Append(Symbol.space);
                await writer.Append(wordsOnTop[i].Text);
            }
        }

        private List<RubyInfo> GetSustituteText(int id)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSubtituteWords),
                id);
            List<RubyInfo> substituteWords = reader.GetClassData<RubyInfo>();
            reader.Close();
            return substituteWords;
        }


        private async Task EndRubySection(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.EndParagraph +
                HTMLTags.EndDiv +
                HTMLTags.EndSection);
        }

        private async Task StartRubySection(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scripture +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader +
                HTMLTags.StartDivWithClass +
                HTMLClasses.textquote +
                Symbol.space +
                HTMLClasses.versetext +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartParagraph
                );
        }

        public List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var result = reader.GetClassData<Keyword>();
            reader.Close();
            return result;
        }


        private async Task ParsePhraseComments(HTMLWriter writer)
        {

        }

        public override Task SetupNextPage()
        {
            int v = citation.CitationRange.FirstVerse + 1;
            int c = citation.CitationRange.FirstChapter;
            int b = citation.CitationRange.Book;
            if (v > Bible.Chapters[b][c])
            {
                c++;
                v = 1;
            }
            if (c > Bible.Chapters[b].Length-1)
            {
                b++;
                if (b > 65)
                {
                    b = 65;
                    c = 22;
                    v = Bible.Chapters[b][c];
                }
                else
                    c = 1;
            }
            citation = new Citation(new KeyID(b, c, v), new KeyID(b, c, v, KeyID.MaxWordIndex));
            return Task.CompletedTask;
        }

        public override Task SetupPrecedingPage()
        {
            int v = citation.CitationRange.FirstVerse - 1;
            int c = citation.CitationRange.FirstChapter;
            int b = citation.CitationRange.Book;
            if (v < 1) c--;
            if (c < 1)
            {
                b--;
                if (b < 0)
                {
                    b = 0;
                    c = 1;
                    v = 1;
                }
            }
            if (c < 1) c = Bible.LastChapterInBook(b);
            if (v < 1) v = Bible.Chapters[b][c];
            citation = new Citation(new KeyID(b, c, v), new KeyID(b, c, v, KeyID.MaxWordIndex));
            return Task.CompletedTask;
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        public override Task LoadTOCInfo(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override Task SetupParentPage()
        {
            citation.CitationType = CitationTypes.Text;
            return Task.CompletedTask;
        }
    }
}
