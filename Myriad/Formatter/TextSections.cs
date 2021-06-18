using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Myriad.Data;
using Feliciana.Data;

namespace Myriad.Formatter
{
    public class TextSections
    {
        bool readingView = false;
        internal bool navigating;
        internal Citation sourceCitation;
        internal Citation targetCitation;
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

        public void SetTargetCitation(Citation targetCitation)
        {
            this.targetCitation = targetCitation;
        }

        internal async Task AddSections(HTMLWriter writer)
        {
            TextSectionFormatter textSectionFormatter = new TextSectionFormatter(writer);
            for (var i = Ordinals.first; i < commentIDs.Count; i++)
            {
                await textSectionFormatter.AddTextSection(this, i);
            }
        }

        internal async Task AddTextSection(HTMLWriter writer)
        {
            TextSectionFormatter textSectionFormatter = new TextSectionFormatter(writer);
            await textSectionFormatter.AddTextSection(this, Ordinals.first);
        }

        internal void GetCommentIDs()
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                sourceCitation.CitationRange.StartID.ID, sourceCitation.CitationRange.EndID.ID);
            var results = reader.GetData<int>();
            reader.Close();
        }
    }
}
