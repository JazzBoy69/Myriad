using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Myriad.Library;

namespace Myriad.Data
{
    public class Keyword : DataObject
    {
        KeyID id;
        int paragraphWordIndex;
        string leadingSymbols;
        string text;
        string trailingSymbols;
        private bool isCapitalized;
        private int mainText;
        private int poetic;

        public async Task Read(DbDataReader reader)
        {
            id = new KeyID(await reader.GetFieldValueAsync<int>(Ordinals.first));
            leadingSymbols = await reader.GetFieldValueAsync<string>(Ordinals.second);
            text = await reader.GetFieldValueAsync<string>(Ordinals.third);
            trailingSymbols = await reader.GetFieldValueAsync<string>(Ordinals.fourth);
            isCapitalized = await reader.GetFieldValueAsync<int>(Ordinals.fifth) != 0;
            mainText = await reader.GetFieldValueAsync<int>(Ordinals.sixth);
            poetic = await reader.GetFieldValueAsync<int>(Ordinals.seventh);
            paragraphWordIndex = await reader.GetFieldValueAsync<int>(Ordinals.eighth);
        }


        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public void ReadSync(DbDataReader reader)
        {
            id = new KeyID( reader.GetFieldValue<int>(Ordinals.first));
            leadingSymbols =  reader.GetFieldValue<string>(Ordinals.second);
            text =  reader.GetFieldValue<string>(Ordinals.third);
            trailingSymbols =  reader.GetFieldValue<string>(Ordinals.fourth);
            isCapitalized =  reader.GetFieldValue<int>(Ordinals.fifth) != 0;
            mainText = reader.GetFieldValue<int>(Ordinals.sixth);
            poetic = reader.GetFieldValue<int>(Ordinals.seventh);
            paragraphWordIndex = reader.GetFieldValue<int>(Ordinals.eighth);
        }

        public ReadOnlySpan<char> LeadingSymbols
        {
            get { return leadingSymbols.AsSpan(); }
        }
        public string LeadingSymbolsString
        {
            get { return leadingSymbols.ToString().Replace('_', ' '); }
        }
        public ReadOnlySpan<char> Text
        {
            get { return text.AsSpan(); }
        }

        public ReadOnlySpan<char> TrailingSymbols
        {
            get { return trailingSymbols.AsSpan(); }
        }

        public string TrailingSymbolString => trailingSymbols;

        public string LeadingSymbolString => leadingSymbols;

        public string TextString => text;

        public bool IsCapitalized
        {
            get { return isCapitalized; }
        }

        public bool IsPoetic
        {
            get { return poetic > 0; }
        }

        public bool PoeticBreak
        {
            get { return poetic == 2; }
        }

        public bool IsMainText
        {
            get { return mainText == 1; }
        }

        public bool StartFootnote
        {
            get { return (mainText == 2) || (mainText == 4) ; }
        }

        public bool EndFootnote
        {
            get { return mainText > 2; }
        }

        public int Chapter
        {
            get { return id.Chapter; }
        }

        public int Book => id.Book;

        public int Verse
        {
            get { return id.Verse; }
        }
        public int WordIndex
        {
            get { return id.WordIndex; }
        }

        public int ID
        {
            get { return id.ID; }
        }

        public int ParagraphWordIndex => paragraphWordIndex;
        public int ParameterCount => throw new NotImplementedException();
    }
}
