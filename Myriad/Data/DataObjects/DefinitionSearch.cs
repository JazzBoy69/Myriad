using Feliciana.Data;
using Feliciana.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class DefinitionSearch : DataObject
    {

        internal DefinitionSearch(MatrixWord matrixWord, int articleID, int paragraphIndex, int sentenceID, int wordIndex)
        {
            ID = articleID;
            ParagraphIndex = paragraphIndex;
            SentenceID = sentenceID;
            WordIndex = wordIndex;
            Start = matrixWord.Start;
            End = matrixWord.End;
            Text = matrixWord.Text;
            Weight = matrixWord.Weight;
            Substitute = matrixWord.Substitute;
        }

        internal DefinitionSearch(SearchWord searchWord, int articleID, int paragraphIndex)
        {
            ID = articleID;
            SentenceID = searchWord.SentenceID;
            WordIndex = searchWord.WordIndex;
            Start = searchWord.Start;
            End = searchWord.End;
            Text = searchWord.Text;
            Weight = searchWord.Weight;
            Substitute = searchWord.Substitute>0;
        }

        public int ID { get; private set; }
        public int ParagraphIndex { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public string Text { get; private set; }
        public int Length { get; private set; }
        public int Weight { get; private set; }
        public bool Substitute { get; private set; }
        public int SentenceID { get; private set; }
        public int WordIndex { get; private set; }

        public int ParameterCount => 9;

        public object GetParameter(int index) //id, paragraphindex, sentence, wordindex, text, weight, start, last, substitute
        {
            switch (index)
            {
                case Ordinals.first:
                    return ID;
                case Ordinals.second:
                    return ParagraphIndex;
                case Ordinals.third:
                    return SentenceID;
                case Ordinals.fourth:
                    return WordIndex;
                case Ordinals.fifth:
                    return Text;
                case Ordinals.sixth:
                    return Weight;
                case Ordinals.seventh:
                    return Start;
                case Ordinals.eighth:
                    return End;
                case Ordinals.ninth:
                    return Substitute ? 1 : 0;
                default:
                    break;
            }
            return null;
        }

        public Task Read(DbDataReader reader)
        {
            throw new NotImplementedException();
        }

        public void ReadSync(DbDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
