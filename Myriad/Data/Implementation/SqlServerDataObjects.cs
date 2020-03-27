using System;
using Myriad.Library;
using System.Data.Common;

namespace Myriad.Data
{
    public class Keyword : DataObject
    {
        KeyID id;
        string leadingSymbols;
        string text;
        string trailingSymbols;
        public void Read(DbDataReader reader)
        {
            id = new KeyID(reader.GetInt32(Ordinals.first));
            leadingSymbols = reader.GetString(Ordinals.second);
            text = reader.GetString(Ordinals.third);
            trailingSymbols = reader.GetString(Ordinals.fourth);
        }
    }
}