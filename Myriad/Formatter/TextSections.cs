using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Formatter
{
    public class TextSections
    {
        bool readingView = false;
        internal bool navigating;
        internal Citation sourceCitation;
        List<int> commentIDs;

        internal bool ReadingView
        {
            get
            {
                return readingView;
            }
        }
        internal List<int> CommentIDs
        {
            get
            {
                return commentIDs;
            }
            set
            {
                commentIDs = value;
                readingView = commentIDs.Count > 1;
            }
        }

    }
}
