using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Parser.Helpers
{
    internal class Formats
    {
        internal bool detail = false;
        internal bool bold = false;
        internal bool super = false;
        internal bool italic = false;
        internal bool heading = false;
        internal bool editable = true;
        internal bool figure = false;
        internal bool labelExists = false;
        internal bool hideDetails = false;
        internal bool sidenote = false;
        internal void Reset()
        {
            detail = false;
            bold = false;
            super = false;
            italic = false;
            heading = false;
            editable = true;
            figure = false;
            labelExists = false;
            hideDetails = false;
            sidenote = false;
        }
    }
}
