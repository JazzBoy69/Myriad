using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Myriad.Data;

namespace Myriad.Search
{
    public class SearchResult : DataObject
    {
        int sentenceID;
        int wordIndex;
        int queryIndex;
        int weight;
        int start;
        int end;
        int substitute;
        string text;
        int length;
        int articleID = -1;

        public int SentenceID => sentenceID;
        public int WordIndex => wordIndex;
        public int QueryIndex => queryIndex;
        public int Weight => weight;
        public int StartID => start;
        public int EndID => end;
        public string Text => text;
        public int ArticleID => articleID;
        public int Length { get { return length; } }
        public bool Substitute { get { return substitute == 1; } }
        public (int, int) Key { get { return (sentenceID, wordIndex); } }
        public int ParameterCount => 7;
        public SearchResult()
        {
        }
        internal SearchResult(int sentenceID, int wordIndex, int length, int queryIndex)
        {
            this.sentenceID = sentenceID;
            this.wordIndex = wordIndex;
            this.queryIndex = queryIndex;
            this.length = length;
        }
        public SearchResult(int sentenceID, int wordIndex, string text, int queryIndex, int weight, int start, int end, int substitute)
        {
            this.sentenceID = sentenceID;
            this.wordIndex = wordIndex;
            this.text = text;
            this.queryIndex = queryIndex;
            this.weight = weight;
            this.start = start;
            this.end = end;
            length = end - start + 1;
            this.substitute = substitute;
        }
        internal SearchResult(MatrixWord word, int sentenceID, int sentenceWordIndex)
        {
            this.sentenceID = sentenceID;
            wordIndex = sentenceWordIndex;
            text = word.Text;
            weight = word.Weight;
            start = word.Start;
            end = word.End;
            substitute = (word.Substitute) ? 1 : 0;
        }


        internal void SetArticleID(int articleID)
        {
            this.articleID = articleID;
        }

        public async Task Read(DbDataReader reader)
        {
            try
            {
                sentenceID = await reader.GetFieldValueAsync<int>(Ordinals.first);
                wordIndex = await reader.GetFieldValueAsync<int>(Ordinals.second);
                length = await reader.GetFieldValueAsync<int>(Ordinals.third);
                queryIndex = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }



        public object GetParameter(int index)
        {
            switch (index)
            {
                case Ordinals.first:
                    return sentenceID;
                case Ordinals.second:
                    return wordIndex;
                case Ordinals.third:
                    return Text;
                case Ordinals.fourth:
                    return weight;
                case Ordinals.fifth:
                    return start;
                case Ordinals.sixth:
                    return end;
                case Ordinals.seventh:
                    return substitute;
                default:
                    break;
            }
            return null;
        }

        public void ReadSync(DbDataReader reader)
        {
            sentenceID = reader.GetFieldValue<int>(Ordinals.first);
            wordIndex = reader.GetFieldValue<int>(Ordinals.second);
            length = reader.GetFieldValue<int>(Ordinals.third);
            queryIndex = reader.GetFieldValue<int>(Ordinals.fourth);
        }
    }
}
