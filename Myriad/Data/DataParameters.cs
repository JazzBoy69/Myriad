using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public interface DataParameters
    {
        abstract string Key1 { get; }
        abstract string Key2 { get; }
        abstract string Key3 { get; }
    }
}
