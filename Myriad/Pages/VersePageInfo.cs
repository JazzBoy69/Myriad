using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Myriad.Data;
using Myriad.Library;

namespace Myriad.Formatter
{
    public class VersePageInfo
    {
        public const ushort originalWordWeight = 200;
        public List<Keyword> keywords;
        List<VerseWord> words;
        List<(int start, int last, int id, int index)> relatedArticles;
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
        public Dictionary<int, (int id, string text)> OriginalWordComments { get; } = new Dictionary<int, (int id, string text)>();
        public SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>> AdditionalArticles { get; } = new SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>>();
        public async Task LoadInfo(Citation citation)
        {
            citation.SetLastWordIndex(await DataRepository.MaxWordIndex(citation.End));
            words = await DataRepository.VerseWords(citation.Start, citation.End);
            FindPhrases(citation);
            await ArrangeRelatedArticles(citation);
            await ArrangeDefinitionSearches(citation);
            await ArrangeCrossReferences(citation);
            await ArrangeOriginalWordComments(citation);
            await ArrangeAdditionalRelatedArticles(citation);
        }

        private async Task ArrangeCrossReferences(Citation citation)
        {
            var crossreferences = await DataRepository.CrossReferencesInRange(citation.Start, citation.End);
            for (int index = Ordinals.first; index<crossreferences.Count; index++)
            {
                var reference = crossreferences[index];
                if (usedReferences.Contains((reference.commentid, reference.paragraphindex)))
                {
                    continue;
                }
                usedReferences.Add((reference.commentid, reference.paragraphindex));
                if ((reference.start < citation.Start) ||
                    (reference.last > citation.End))
                //reference starts at a preceeding verse
                {
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.commentid);
                    if (reference.start < citation.Start)
                    {
                        // add to See Also
                        if (links.Count > 0)
                        {
                            if (AdditionalCrossReferences.ContainsKey(links.First()))
                                AdditionalCrossReferences[links.First()].Add((reference.commentid,
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
                if ((reference.start == citation.Start) &&
                    (reference.last == citation.EndID.ID))
                {
                    bottom = -1;
                    top = -1;
                }

                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].Last < reference.start)
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
                        (reference.last <= Phrases[mid].Last))
                    {
                        phraseIndex = mid;
                        AddOriginalWordCrossReference(reference, mid);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom>-1) && (bottom < Phrases.Count)) &&
                    (((reference.start >= Phrases[bottom].Start) &&
                        (reference.last <= Phrases[bottom].Last))))
                {
                    AddOriginalWordCrossReference(reference, bottom);
                    phraseIndex = bottom;
                }
                if (phraseIndex == -1)
                {
                    //if suppressing and not first verse in cross reference add to "See also"
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.commentid);
                    if (links.Count > 0)
                    {
                        AddToAdditionalCrossReferences(reference.commentid, reference.paragraphindex, links.First(),
                            (reference.start < citation.Start));
                    }
                }
            }
        }

        private async Task ArrangeOriginalWordComments(Citation citation)
        {
            var originalWordComments = await DataRepository.OriginalWordComments(citation.Start, citation.End);
            for (int index = Ordinals.first; index< originalWordComments.Count; index++)
            {
                var reference = originalWordComments[index];
                if (OriginalWordsInRange(reference.start, reference.last).Length == Number.nothing) continue;
                if ((reference.start < citation.Start) ||
                    (reference.last > citation.End))
                {
                    continue;
                }
                int bottom = Ordinals.first;
                int top = Phrases.Count - 1;
                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].Last < reference.start)
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
                        (reference.last <= Phrases[mid].Last))
                    {
                        phraseIndex = mid;
                        AddOriginalWordComment(reference.id, reference.text, mid);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom < Phrases.Count)) &&
                    (((reference.start >= Phrases[bottom].Start) &&
                        (reference.last <= Phrases[bottom].Last))))
                {
                    AddOriginalWordComment(reference.id, reference.text, bottom);
                    phraseIndex = bottom;
                }
            }
        }

        private void AddOriginalWordComment(int id, string text, int index)
        {
            if (OriginalWordComments.ContainsKey(index))
            {
                return;
            }
            OriginalWordComments.Add(index, (id, text));
        }

        public string OriginalWordsInRange(int start, int end)
        {
            var originalWordsInPhrase = (from w in OriginalWords
                                                        where w.Start >= start &&
                                                        w.Last <= end
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

        private void AddOriginalWordCrossReference((int start, int last, int commentid, int paragraphindex) reference, int index)
        {
            if (OriginalWordCrossReferences.ContainsKey(index))
                OriginalWordCrossReferences[index].Add((reference.commentid, reference.paragraphindex));
            else
                OriginalWordCrossReferences.Add(index, new List<(int articleID, int paragraphIndex)>() {
                    (reference.commentid, reference.paragraphindex)
                });
        }
        private void AddToAdditionalCrossReferences(int articleid, int paragraphindex, (int start, int end) range, bool show)
        {
            if (AdditionalCrossReferences.ContainsKey(range))
                AdditionalCrossReferences[range].Add(
                    (articleid, paragraphindex, show));
            else
                AdditionalCrossReferences.Add(range,
                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                    (articleid, paragraphindex, false)});
        }

        private void FindPhrases(Citation citation)
        {
            OriginalWords = from w in words
                            where w.Weight == originalWordWeight
                            select w;
            int start = citation.Start;

            var lastPhrase = new VerseWord();
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                var word = words[index];

                if ((lastPhrase.Weight == originalWordWeight) && 
                    (word.Start < lastPhrase.Last) && 
                    (word.Start > lastPhrase.Start) &&
                    (word.Weight == originalWordWeight))
                {
                    if (Ellipses.ContainsKey(lastPhrase.Start))
                    {
                        Ellipses[lastPhrase.Start].ExtendTo(word.Last);
                    }
                    else
                        Ellipses.Add(lastPhrase.Start, new CitationRange(word.Start, word.Last));
                    Phrases.Add(word);
                    continue;
                }
                if (word.Start < start)
                {
                    continue;
                }
                var origwords = from w in OriginalWords
                                where w.Last >= word.Start &&
                                w.Start <= word.Last
                                select w;
                if (origwords.Any())
                {
                    word.SetStart(Math.Min(word.Start, origwords.Min(w => w.Start)));
                    word.SetEnd(Math.Max(word.Last, origwords.Max(w => w.Last)));
                }
                Phrases.Add(word);
                if (word.Last - word.Start > 0)
                {
                    var owords = from w in OriginalWords
                                 where w.Start == word.Start &&
                                 w.Last == word.Last
                                 select w;
                    if (owords.Any())
                        lastPhrase = owords.First();
                }
                start = word.Last + 1;
            }
        }
        private async Task ArrangeDefinitionSearches(Citation citation)
        {
            var definitionSearches = await DataRepository.DefinitionSearchesInRange(citation.Start, citation.End);
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

        private async Task ArrangeRelatedArticles(Citation citation)
        {
            relatedArticles = await DataRepository.RelatedArticles(citation.Start, citation.End);
            for (int i=Ordinals.first; i<relatedArticles.Count; i++)
            {
                if (usedArticles.Contains((relatedArticles[i].id, relatedArticles[i].index)))
                {
                    continue;
                }
                if (((relatedArticles[i].last- relatedArticles[i].start)<10) && (relatedArticles[i].start >= citation.Start) && 
                    (relatedArticles[i].last <= citation.End) &&
                        ((relatedArticles[i].start != citation.Start) || (relatedArticles[i].last != citation.End)))
                { // Add an article reference to a single phrase in verse to definition searches
                    await ArrangeDefinitionSearch((relatedArticles[i].start, relatedArticles[i].last, relatedArticles[i].id));
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
                if (Phrases[mid].Last < item.start)
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
                    (Ellipses[Phrases[mid].Start].Contains((item.start, item.end))))
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
                                                  where paragraph.id == commentInfo.articleID
                                                  orderby paragraph.index
                                                  select paragraph.index).Distinct().ToList();
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

        private async Task ArrangeAdditionalRelatedArticles(Citation citation)
        {
            //Loop through Related Articles to add unused articles to Additional References or "See also"
            int index = Ordinals.first;
            while (index < relatedArticles.Count)
            {
                if (usedArticles.Contains((relatedArticles[index].id, relatedArticles[index].index)))
                {
                    index++;
                    continue;
                }
                usedArticles.Add((relatedArticles[index].id, relatedArticles[index].index));

                if (relatedArticles[index].start < citation.Start)
                {
                    await AddToAdditionalArticles(index, true);
                }
                else
                {
                    await AddToAdditionalArticles(index, 
                        await DefinitionSearchExistsInRange(
                            relatedArticles[index].start, 
                            relatedArticles[index].last,
                            relatedArticles[index].id)
                    );
                }
                index++;
            }
        }

        private async Task<bool> DefinitionSearchExistsInRange(int start, int last, int articleID)
        {
            List<(int start, int end)> ranges = await DataRepository.DefinitionSearchesInRange(start, last, articleID);
            return ranges.Count > Number.nothing;
        }

        private async Task AddToAdditionalArticles(int index, bool show)
        {
            string title = await ArticleTitle(relatedArticles[index].id);
            if (AdditionalArticleIDs.ContainsKey(title))
                AdditionalArticles[title].Add((relatedArticles[index].id, relatedArticles[index].index,
                    show));
            else
            {
                AdditionalArticles.Add(title, new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                            (relatedArticles[index].id, relatedArticles[index].index, show)});
                AdditionalArticleIDs.Add(title, relatedArticles[index].id);
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
