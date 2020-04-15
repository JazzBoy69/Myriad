using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;

namespace Myriad.Search
{
    public class SearchSentence
    {
        readonly private int sentenceID;
        private int score;
        readonly List<List<SearchResult>> wordLists = new List<List<SearchResult>>();
        readonly List<WordPosition> wordPositions = new List<WordPosition>();
        readonly List<int> wordCounts = new List<int>();
        List<SearchResult> words = new List<SearchResult>();
        readonly private int wordCount;
        int totalWeight = 0;
        private int type;
        private int space;

        public int Space
        { get { return space; } }

        public int Type { get { return type; } internal set { type = value; } }
        public SearchSentence(int sentenceID, int wordCount)
        {
            this.sentenceID = sentenceID;
            this.wordCount = wordCount;
            int index = Ordinals.first;
            while (index < wordCount)
            {
                wordLists.Add(new List<SearchResult>());
                wordCounts.Add(0);
                index++;
            }
        }

        public SearchSentence(int sentenceID, int wordCount, int sentenceType)
        {
            type = sentenceType;
            this.sentenceID = sentenceID;
            this.wordCount = wordCount;
            int index = Ordinals.first;
            while (index < wordCount)
            {
                wordLists.Add(new List<SearchResult>());
                wordCounts.Add(0);
                index++;
            }
        }

        public int Score
        {
            get { return score; }
        }

        /// <summary>
        /// Add a search hit to the search sentence
        /// </summary>
        /// <param name="searchResult"></param>
        /// The search result to add to this sentence
        ///         
        internal void Add(SearchResult searchResult)
        {
            try
            {
                if (searchResult.QueryIndex >= wordLists.Count) return;
                totalWeight += searchResult.Weight;
                wordLists[searchResult.QueryIndex].Add(searchResult);
                wordCounts[searchResult.QueryIndex]++;
                wordPositions.Add(new WordPosition(searchResult.WordIndex,
                    searchResult.QueryIndex, searchResult.Length));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        internal void Add(SearchResult searchResult, int queryIndex)
        {
            if (queryIndex >= wordLists.Count) return;
            totalWeight += searchResult.Weight;
            wordLists[queryIndex].Add(searchResult);
            wordCounts[queryIndex]++;
            wordPositions.Add(new WordPosition(searchResult.WordIndex, queryIndex, searchResult.Length));
        }

        internal void CalculateScoreNew()
        {
            if (wordCount > 1)
            {
                //If there is a word missing return a really bad score
                foreach (int count in wordCounts)
                {
                    if (count == 0)
                    {
                        score = -20000;
                        return;
                    }
                }

                int extra = wordPositions.Count - wordCount;
                wordPositions.Sort();
                //Find smallest union of search phrases
                if (extra > 0)
                {
                    WordPosition first = wordPositions.First();
                    WordPosition last = wordPositions.Last();
                    while ((wordPositions.Count > wordCount) && ((wordCounts[first.QueryIndex] > 1) ||
                        (wordCounts[last.QueryIndex] > 1)))
                    {
                        int low = 1000000;
                        int high = 1000000;
                        if (wordCounts[first.QueryIndex] > 1)
                        {
                            low = last.WordIndex - wordPositions.ElementAt(Ordinals.second).WordIndex;
                        }
                        if (wordCounts[last.QueryIndex] > 1)
                        {
                            high = wordPositions.ElementAt(wordPositions.Count - 2).WordIndex - first.WordIndex;
                        }
                        if (low < high)
                        {
                            wordCounts[first.QueryIndex]--;
                            wordPositions.RemoveAt(Ordinals.first);
                            first = wordPositions.First();
                        }
                        else
                        {
                            wordCounts[last.QueryIndex]--;
                            wordPositions.RemoveAt(wordPositions.Count - 1);
                            last = wordPositions.Last();
                        }
                    }
                }
                int coverage = 0;
                foreach (WordPosition entry in wordPositions) coverage += entry.Length;
                int space = (wordPositions.Last().WordIndex - wordPositions.First().WordIndex + 1) - coverage;
                //test how ordered the results are
                int disorder = EditDistance();
                if ((space <= 0) && (disorder == 0))
                    score = 100000000 + totalWeight;
                else
                    score = 20000 + totalWeight - (space * 1000) - (disorder * 1000);
            }
            else //calculate score for single word query
            {
                score = 5000;
                if (wordLists.Count > Number.nothing)
                {
                    foreach (SearchResult word in wordLists[Ordinals.first])
                    {
                        score += word.Weight;
                    }
                }
            }
        }

        internal void CalculateDistance()
        {
            if (wordCount == 1) //todo weight single word searches
            {
                score = 0;
                return;
            }
            try
            {
                int extra = wordPositions.Count - wordCount;
                wordPositions.Sort();
                //Find smallest union of search phrases
                if (extra > 0)
                {
                    WordPosition first = wordPositions.First();
                    WordPosition last = wordPositions.Last();
                    while ((wordPositions.Count > wordCount) && ((wordCounts[first.QueryIndex] > 1) ||
                        (wordCounts[last.QueryIndex] > 1)))
                    {
                        int low = 1000000;
                        int high = 1000000;
                        if (wordCounts[first.QueryIndex] > 1)
                        {
                            low = last.WordIndex - wordPositions.ElementAt(Ordinals.second).WordIndex;
                        }
                        if (wordCounts[last.QueryIndex] > 1)
                        {
                            high = wordPositions.ElementAt(wordPositions.Count - 2).WordIndex - first.WordIndex;
                        }
                        if (low < high)
                        {
                            wordCounts[first.QueryIndex]--;
                            wordPositions.RemoveAt(Ordinals.first);
                            first = wordPositions.First();
                        }
                        else
                        {
                            wordCounts[last.QueryIndex]--;
                            wordPositions.RemoveAt(wordPositions.Count - 1);
                            last = wordPositions.Last();
                        }
                    }
                }
                int coverage = 0;
                foreach (WordPosition entry in wordPositions) coverage += entry.Length;
                space = (wordPositions.Last().WordIndex - wordPositions.First().WordIndex + 1) - coverage;
                if ((space < 0) || (space > 7)) space = 300;
                //test how ordered the results are
                int disorder = EditDistance();
                score = space + disorder;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal int FirstPosition
        {
            get
            {
                return wordPositions[Ordinals.first].WordIndex;
            }
        }

        internal int LastPosition
        {
            get
            {
                if (wordPositions.Count == 0) return Result.notfound;
                int result = wordPositions[Ordinals.last].WordIndex;
                return result;
            }
        }

        private int EditDistance()
        {
            int n = wordPositions.Count;
            int m = wordCount;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = ((j - 1) == wordPositions.ElementAt(i - 1).QueryIndex) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m] - (wordPositions.Count - wordCount);
        }




        internal const int missingPenalty = -50000;

        internal List<SearchResult> Words
        {
            get
            {
                if (words.Count > 0) return words;
                foreach (List<SearchResult> list in wordLists)
                {
                    words.AddRange(list);
                }
                words = words.OrderBy(w => w.WordIndex).ToList();
                return words;
            }
        }

        public int SentenceID { get { return sentenceID; } }

        public int WordsCount { get { return words.Count; } }
    }

}
