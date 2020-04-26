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
        private bool isPoetic;

        public async Task Read(DbDataReader reader)
        {
            id = new KeyID(await reader.GetFieldValueAsync<int>(Ordinals.first));
            leadingSymbols = await reader.GetFieldValueAsync<string>(Ordinals.second);
            text = await reader.GetFieldValueAsync<string>(Ordinals.third);
            trailingSymbols = await reader.GetFieldValueAsync<string>(Ordinals.fourth);
            isCapitalized = await reader.GetFieldValueAsync<int>(Ordinals.fifth) != 0;
            isPoetic = await reader.GetFieldValueAsync<int>(Ordinals.sixth) != 0;
            paragraphWordIndex = await reader.GetFieldValueAsync<int>(Ordinals.seventh);
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
            isPoetic =  reader.GetFieldValue<int>(Ordinals.sixth) != 0;
            paragraphWordIndex =  reader.GetFieldValue<int>(Ordinals.seventh);
        }

        public ReadOnlySpan<char> LeadingSymbols
        {
            get { return leadingSymbols.AsSpan(); }
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
            get { return isPoetic; }
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

        public int ParameterCount => throw new NotImplementedException();
    }
}
