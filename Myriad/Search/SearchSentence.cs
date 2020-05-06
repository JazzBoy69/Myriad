using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Myriad.Data;

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

        public int Type { get { return type; } }
        public int WordCount => wordCount;
        public List<int> WordCounts => wordCounts;

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
        public void SetType(int typeNumber)
        {
            type = typeNumber;
        }

        public void SetScore(int score)
        {
            this.score = score;
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
            if (searchResult.QueryIndex >= wordLists.Count) return;
            wordLists[searchResult.QueryIndex].Add(searchResult);
            wordCounts[searchResult.QueryIndex]++;
            wordPositions.Add(new WordPosition(searchResult.WordIndex,
                searchResult.QueryIndex, searchResult.Length));
        }
        internal void Add(SearchResult searchResult, int queryIndex)
        {
            if (queryIndex >= wordLists.Count) return;
            totalWeight += searchResult.Weight;
            wordLists[queryIndex].Add(searchResult);
            wordCounts[queryIndex]++;
            wordPositions.Add(new WordPosition(searchResult.WordIndex, queryIndex, searchResult.Length));
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

        public List<WordPosition> WordPositions => wordPositions;

        internal void Sort()
        {
            wordPositions.Sort();
        }
    }

}
