using Feliciana.Data;
using Feliciana.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class RubyInfo : DataObject
    {
        string ruby;
        int lastRuby;

        public string Text => ruby;
        public int EndID => lastRuby;
        public int ParameterCount => 2;

        public RubyInfo()
        {

        }
        public RubyInfo(string rubyText, int lastRuby, int weight)
        {
            ruby = rubyText;
            this.lastRuby = lastRuby;
        }

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            ruby = await reader.GetFieldValueAsync<string>(Ordinals.first);
            lastRuby = await reader.GetFieldValueAsync<int>(Ordinals.second);
        }

        public void ReadSync(DbDataReader reader)
        {
            ruby = reader.GetFieldValue<string>(Ordinals.first);
            lastRuby = reader.GetFieldValue<int>(Ordinals.second);
        }
    }
}