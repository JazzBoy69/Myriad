using System;
using System.Collections.Generic;

namespace Myriad.ToApplied

{
    public interface DataReader
    {
        internal List<string> GetData(string key);
    }

}