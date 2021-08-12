using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Myriad.Data;
using Myriad.Library;

namespace Myriad.Formatter
{
    public class VersePageInfo
    {
        public const ushort originalWordWeight = 200;
        public List<Keyword> keywords;
        List<VerseWord> words;
        List<RangeAndParagraph> relatedArticles;
        List<(int commentID, int paragraphIndex)> usedReferences = new List<(int articleID, int paragraphIndex)>();
        readonly List<(int articleID, int paragraphIndex)> usedArticles = new List<(int articleID, int paragraphIndex)>();

        public List<VerseWord> Phrases { get; } = new List<VerseWord>();
        public IEnumerable<VerseWord> OriginalWords { get; private set; }
        public Dictionary<int, CitationRange> Ellipses { get; } = new Dictionary<int, CitationRange>();
        public Dictionary<int, Dictionary<string, ((int start, int end) range, int articleID, 
            List<int> paragraphIndices)>> PhraseArticles { get; } = 
            new Dictionary<int, Dictionary<string, ((int start, int end) range, 
                int articleID, List<int> paragraphIndices)>>();
        public Dictionary<string, int> AdditionalArticleIDs { get; } = new Dictionary<string, int>();
        public SortedDictionary<(int startID, int endID), 
            List<(int articleID, int paragraphIndex, bool suppressed)>> 
            AdditionalCrossReferences { get; } = 
            new SortedDictionary<(int startID, int endID), 
                List<(int articleID, int paragraphIndex, bool suppressed)>>();
        public VersePageInfo()
        {

        }

        public Dictionary<int, List<(int articleID, int paragraphIndex)>> OriginalWordCrossReferences { get; } = new Dictionary<int, List<(int articleID, int paragraphIndex)>>();
        public Dictionary<int, List<(int articleID, int paragraphIndex)>> OriginalWordComments { get; } = new Dictionary<int, List<(int articleID, int paragraphIndex)>>();
        public SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>> AdditionalArticles { get; } = new SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>>();
        public async Task LoadInfo(CitationRange citationRange)
        {
            await citationRange.ResolveLastWordIndex();
            ReadSearchWords(citationRange);
            FindPhrases(citationRange);
            await ArrangeRelatedArticles(citationRange);
            await ArrangeDefinitionSearches(citationRange);
            await ArrangeCrossReferences(citationRange);
            ArrangeOriginalWordComments(citationRange);
            await ArrangeAdditionalRelatedArticles(citationRange);
        }

        private async Task ArrangeCrossReferences(CitationRange citationRange)
        {
            var crossreferences = await DataRepository.CrossReferencesInRange(citationRange.StartID.ID, citationRange.EndID.ID);
            reader.Close();
            for (int index = Ordinals.first; index<crossreferences.Count; index++)
            {
                var reference = crossreferences[index];
                if (usedReferences.Contains(reference.Key))
                {
                    continue;
                }
                usedReferences.Add(reference.Key);
                if ((reference.start < citationRange.StartID.ID) ||
                    (reference.last > citationRange.EndID.ID))
                //reference starts at a preceeding verse
                {
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.ArticleID);
                    if (reference.start < citationRange.StartID.ID)
                    {
                        // add to See Also
                        if (links.Count > 0)
                        {
                            if (AdditionalCrossReferences.ContainsKey(links.First()))
                                AdditionalCrossReferences[links.First()].Add((reference.ArticleID,
                                    reference.paragraphindex, true));
                            else
                                AdditionalCrossReferences.Add(links.First(),
                                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                            (reference.commentid, reference.paragraphindex, true)});
                        }
                    }
                    else
                    {
                        if (links.Count > 0)
                        {
                            if (AdditionalCrossReferences.ContainsKey(links.First()))
                                AdditionalCrossReferences[links.First()].Add((reference.commentid,
                                    reference.paragraphindex, false));
                            else
                                AdditionalCrossReferences.Add(links.First(),
                                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                            (reference.commentid, reference.paragraphindex, false)});
                        }
                    }
                    continue;
                }
                int bottom = Ordinals.first;
                int top = Phrases.Count - 1;
                if ((reference.start == citationRange.StartID.ID) &&
                    (reference.last == citationRange.EndID.ID))
                {
                    bottom = -1;
                    top = -1;
                }

                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].End < reference.start)
                    {
                        if (bottom == mid) bottom++; else bottom = mid;
                        continue;
                    }
                    if (Phrases[mid].Start > reference.last)
                    {
                        if (top == mid) top--; else top = mid;
                        continue;
                    }
                    if ((reference.start >= Phrases[mid].Start) &&
                        (reference.last <= Phrases[mid].End))
                    {
                        phraseIndex = mid;
                        AddOriginalWordCrossReference(reference, mid);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom>-1) && (bottom < Phrases.Count)) &&
                    (((reference.start >= Phrases[bottom].Start) &&
                        (reference.last <= Phrases[bottom].End))))
                {
                    AddOriginalWordCrossReference(reference, bottom);
                    phraseIndex = bottom;
                }
                if (phraseIndex == -1)
                {
                    //if suppressing and not first verse in cross reference add to "See also"
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.ArticleID);
                    if (links.Count > 0)
                    {
                        AddToAdditionalCrossReferences(reference, links.First(),
                            (reference.start < citationRange.StartID.ID));
                    }
                }
            }
        }

        private void ArrangeOriginalWordComments(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadLinkedParagraphs),
                citationRange.StartID.ID, citationRange.EndID.ID); 
            List<RangeAndParagraph> linkedParagraphs = reader.GetClassData<RangeAndParagraph>();
            reader.Close();
            for (int index = Ordinals.first; index<linkedParagraphs.Count; index++)
            {
                var reference = linkedParagraphs[index];
                if (usedReferences.Contains(reference.Key))
                {
                    continue;
                }
                usedReferences.Add(reference.Key);
                if (OriginalWordsInRange(reference.Start, reference.End).Length == Number.nothing) continue;
                if ((reference.Start < citationRange.StartID.ID) ||
                    (reference.End > citationRange.EndID.ID))
                {
                    continue;
                }
                int bottom = Ordinals.first;
                int top = Phrases.Count - 1;
                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].End < reference.Start)
                    {
                        if (bottom == mid) bottom++; else bottom = mid;
                        continue;
                    }
                    if (Phrases[mid].Start > reference.End)
                    {
                        if (top == mid) top--; else top = mid;
                        continue;
                    }
                    if ((reference.Start >= Phrases[mid].Start) &&
                        (reference.End <= Phrases[mid].End))
                    {
                        phraseIndex = mid;
                        AddOriginalWordComment(reference, mid);
                        usedReferences.Add(reference.Key);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom < Phrases.Count)) &&
                    (((reference.Start >= Phrases[bottom].Start) &&
                        (reference.End <= Phrases[bottom].End))))
                {
                    AddOriginalWordComment(reference, bottom);
                    phraseIndex = bottom;
                }
            }
        }

        private void AddOriginalWordComment(RangeAndParagraph rangeAndParagraph, int index)
        {
            if (OriginalWordComments.ContainsKey(index))
            {
                OriginalWordComments[index].Add(rangeAndParagraph.Key);
                return;
            }
            OriginalWordComments.Add(index, new List<(int articleID, int paragraphIndex)>()
                { rangeAndParagraph.Key });
        }

        public string OriginalWordsInRange(int start, int end)
        {
            var originalWordsInPhrase = (from w in OriginalWords
                                                        where w.Start >= start &&
                                                        w.End <= end
                                                        select w.Text).ToList();
            int count = originalWordsInPhrase.Count();
            if (count == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i=Ordinals.first; i<originalWordsInPhrase.Count; i++)
            {
                if (i > Ordinals.first) sb.Append(' ');
                sb.Append(originalWordsInPhrase[i]);
            }
            return sb.ToString();
        }

        private void AddOriginalWordCrossReference((int start, int last, int commentid, int paragraphindex) rangeAndParagraph, int index)
        {
            if (OriginalWordCrossReferences.ContainsKey(index))
                OriginalWordCrossReferences[index].Add(rangeAndParagraph.Key);
            else
                OriginalWordCrossReferences.Add(index, new List<(int articleID, int paragraphIndex)>() {
                    rangeAndParagraph.Key
                });
        }
        private void AddToAdditionalCrossReferences(RangeAndParagraph reference, (int start, int end) range, bool show)
        {
            if (AdditionalCrossReferences.ContainsKey(range))
                AdditionalCrossReferences[range].Add(
                    (reference.ArticleID, reference.ParagraphIndex, show));
            else
                AdditionalCrossReferences.Add(range,
                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                    (reference.ArticleID, reference.ParagraphIndex, false)});
        }

        private void ReadSearchWords(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadVerseWords),
                citationRange.StartID.ID, citationRange.EndID.ID);
            words = reader.GetClassData<VerseWord>();
            reader.Close();
        }

        private void FindPhrases(CitationRange citationRange)
        {
            OriginalWords = from w in words
                            where w.Weight == originalWordWeight
                            select w;
            int start = citationRange.StartID.ID;

            var lastPhrase = new VerseWord();
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                var word = words[index];

                if ((lastPhrase.Weight == originalWordWeight) && 
                    (word.Start < lastPhrase.End) && 
                    (word.Start > lastPhrase.Start) &&
                    (word.Weight == originalWordWeight))
                {
                    if (Ellipses.ContainsKey(lastPhrase.Start))
                    {
                        Ellipses[lastPhrase.Start].ExtendTo(word.End);
                    }
                    else
                        Ellipses.Add(lastPhrase.Start, new CitationRange(word.Start, word.End));
                    Phrases.Add(word);
                    continue;
                }
                if (word.Start < start)
                {
                    continue;
                }
                var origwords = from w in OriginalWords
                                where w.End >= word.Start &&
                                w.Start <= word.End
                                select w;
                if (origwords.Any())
                {
                    word.SetStart(Math.Min(word.Start, origwords.Min(w => w.Start)));
                    word.SetEnd(Math.Max(word.End, origwords.Max(w => w.End)));
                }
                Phrases.Add(word);
                if (word.End - word.Start > 0)
                {
                    var owords = from w in OriginalWords
                                 where w.Start == word.Start &&
                                 w.End == word.End
                                 select w;
                    if (owords.Any())
                        lastPhrase = owords.First();
                }
                start = word.End + 1;
            }
        }
        private async Task ArrangeDefinitionSearches(CitationRange citationRange)
        {
            var definitionSearches = await ReadDefinitionSearchesInVerse(citationRange.StartID.ID, citationRange.EndID.ID);
            var used = new List<(int start, int end, int id)>();
            for (int index = Ordinals.first; index < definitionSearches.Count; index++) 
            {
                if (!used.Contains(definitionSearches[index]))
                {
                    used.Add(definitionSearches[index]);
                }
                await ArrangeDefinitionSearch(definitionSearches[index]);
            }
        }
        private static async Task<List<(int start, int end, int id)>> ReadDefinitionSearchesInVerse(int start, int end)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearchesInVerse),
                start, end);
            List<(int start, int end, int id)> definitionsearches = await reader.GetData<int, int, int>();
            reader.Close();
            return definitionsearches;
        }

        internal static async Task<List<(int start, int end, int id)>> ReadDefinitionSearches(int start)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearches),
                start);
            List<(int start, int end, int id)> definitionsearches = await reader.GetData<int, int, int>();
            reader.Close();
            return definitionsearches;
        }

        private async Task ArrangeRelatedArticles(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadRelatedArticles),
                citationRange.StartID.ID, citationRange.EndID.ID);
            relatedArticles = reader.GetClassData<RangeAndParagraph>();
            reader.Close();
            for (int i=Ordinals.first; i<relatedArticles.Count; i++)
            {
                if (usedArticles.Contains(relatedArticles[i].Key))
                {
                    continue;
                }
                if (((relatedArticles[i].End- relatedArticles[i].Start)<10) && (relatedArticles[i].Start >= citationRange.StartID.ID) && 
                    (relatedArticles[i].End <= citationRange.EndID.ID) &&
                        ((relatedArticles[i].Start != citationRange.StartID.ID) || (relatedArticles[i].End != citationRange.EndID.ID)))
                { // Add an article reference to a single phrase in verse to definition searches
                    await ArrangeDefinitionSearch((relatedArticles[i].Start, relatedArticles[i].End, relatedArticles[i].ArticleID));
                    continue;
                }
            }
        }

        private async Task ArrangeDefinitionSearch((int start, int end, int id) item)
        {
            int bottom = Ordinals.first;
            int top = Phrases.Count - 1;
            bool found = false;
            while (bottom <= top)
            {
                int mid = (top - bottom) / 2 + bottom;
                if (Phrases[mid].End < item.start)
                {
                    if (bottom == mid) bottom++; else bottom = mid;
                    continue;
                }
                if (Phrases[mid].Start > item.end)
                {
                    if (top == mid) top--; else top = mid;
                    continue;
                }
                if ((Ellipses.ContainsKey(Phrases[mid].Start)) &&
                    (Ellipses[Phrases[mid].Start].Contains(new CitationRange(item.start, item.end))))
                {
                    if (bottom == mid) bottom++; else bottom = mid;
                    continue;
                }
                found = true;
                await AddComment(((item.start, item.end), item.id), mid);
                break;
            }
            if ((!found) && (bottom < Phrases.Count))
            {
                await AddComment(((item.start, item.end), item.id), bottom);
                found = true;
            }
        }


        private async Task AddComment(((int start, int end) range, int articleID) commentInfo, int index)
        {
            //Add related article
            List<(int articleID, int paragraphIndex)> result = new List<(int articleID, int paragraphIndex)>();
            var articleParagraphs = (from paragraph in relatedArticles
                                                  where paragraph.ArticleID == commentInfo.articleID
                                                  orderby paragraph.ParagraphIndex
                                                  select paragraph.ParagraphIndex).Distinct().ToList();
            List<int> newParagraphs = new List<int>();
            for (int i=Ordinals.first; i<articleParagraphs.Count; i++)
            {
                if (usedArticles.Contains((commentInfo.articleID, articleParagraphs[i]))) continue;
                newParagraphs.Add(articleParagraphs[i]);
                usedArticles.Add((commentInfo.articleID, articleParagraphs[i]));
            }
            if (newParagraphs.Count == 0) return;
            if (PhraseArticles.ContainsKey(index))
            {
                string articleTitle = await ArticleTitle(commentInfo.articleID);
                if (!PhraseArticles[index].ContainsKey(articleTitle))
                {
                    PhraseArticles[index].Add(articleTitle,
                        (commentInfo.range, commentInfo.articleID, newParagraphs));
                }
            }
            else
            {
                PhraseArticles.Add(index, new Dictionary<string, ((int start, int end) range, int articleID, List<int> paragraphIndices)>());
                PhraseArticles[index].Add(await ArticleTitle(commentInfo.articleID),
                    (commentInfo.range, commentInfo.articleID, newParagraphs));
            }
        }

        private async Task ArrangeAdditionalRelatedArticles(CitationRange citationRange)
        {
            //Loop through Related Articles to add unused articles to Additional References or "See also"
            int index = Ordinals.first;
            while (index < relatedArticles.Count)
            {
                if (usedArticles.Contains(relatedArticles[index].Key))
                {
                    index++;
                    continue;
                }
                usedArticles.Add(relatedArticles[index].Key);

                if (relatedArticles[index].Start < citationRange.StartID.ID)
                {
                    await AddToAdditionalArticles(index, true);
                }
                else
                {
                    await AddToAdditionalArticles(index, await DefinitionSearchExistsInRange(relatedArticles[index].Range,
                       relatedArticles[index].ArticleID));
                }
                index++;
            }
        }

        private async Task<bool> DefinitionSearchExistsInRange((int start, int end) range, int articleID)
        {
            var reader = new DataReaderProvider<int, int, int>(SqlServerInfo.GetCommand(DataOperation.DefinitionSearchesInRange),
                range.start, range.end, articleID);
            List<(int start, int end)> ranges = await reader.GetData<int, int>();
            reader.Close();
            return ranges.Count > Number.nothing;
        }

        private async Task AddToAdditionalArticles(int index, bool show)
        {
            string title = await ArticleTitle(relatedArticles[index].ArticleID);
            if (AdditionalArticleIDs.ContainsKey(title))
                AdditionalArticles[title].Add((relatedArticles[index].ArticleID, relatedArticles[index].ParagraphIndex,
                    show));
            else
            {
                AdditionalArticles.Add(title, new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                            (relatedArticles[index].ArticleID, relatedArticles[index].ParagraphIndex, show)});
                AdditionalArticleIDs.Add(title, relatedArticles[index].ArticleID);
            }
        }

        private static async Task<string> ArticleTitle(int articleID)
        {
            if (articleID == Number.nothing) return "";
            string title = await DataRepository.Title(articleID);
            return title.Replace(' ', '_').Replace('\u0a00', '_');
        }

        public bool PhraseHasNoComments(int index)
        {
            return (!OriginalWordComments.ContainsKey(index) && 
                !PhraseArticles.ContainsKey(index) && 
                !OriginalWordCrossReferences.ContainsKey(index));
        }
    }
}
