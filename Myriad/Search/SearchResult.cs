using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;

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
            this.queryIndex = queryIndex;
            this.weight = weight;
            this.start = start;
            this.end = end;
            length = end - start + 1;
            this.substitute = substitute;
            this.text = text;
        }

        public int Length { get { return length; } }

        public bool Substitute { get { return substitute == 1; } }

        public (int, int) Key { get { return (sentenceID, wordIndex); } }

        public int ParameterCount => throw new NotImplementedException();


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



        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
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
