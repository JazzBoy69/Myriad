using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using System.Data.Common;
using System.Data.SqlClient;

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

        public void Read(DbDataReader reader)
        {
            id = new KeyID(reader.GetInt32(Ordinals.first));
            leadingSymbols = reader.GetString(Ordinals.second);
            text = reader.GetString(Ordinals.third);
            trailingSymbols = reader.GetString(Ordinals.fourth);
            isCapitalized = reader.GetInt32(Ordinals.fifth) != 0;
            isPoetic = reader.GetInt32(Ordinals.sixth) != 0;
            paragraphWordIndex = reader.GetInt32(Ordinals.seventh);
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public void AddParameterTo<DataType>(DataWriter<DataType> writer, int index) where DataType : DataObject
        {
            throw new NotImplementedException();
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
