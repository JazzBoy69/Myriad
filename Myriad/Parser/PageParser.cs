﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class PageParser : MarkupParser
    {
        public PageParser(HTMLResponse builder) : base(builder)
        {
        }

        protected override void HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            AddHTMLBeforeParagraph();
            if (currentParagraph.Length > 1)
            {
                foundEndToken = HandleStartToken();
            }
            if (!formats.heading && !formats.figure)
            {
                formatter.StartParagraph();
                if (formats.editable)
                {
                    formatter.StartEditSpan(paragraphInfo);
                }
            }
        }

        protected override void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            if (citationLevel > 0)
            {
                mainRange.MoveStartTo(lastDash + 1);
                mainRange.PullEnd();
                HandleLastDashCitations();
                mainRange.BumpEnd();
            }
            formatter.AppendString(currentParagraph, mainRange);

            HandleEditParagraphSpan();

            if (formats.heading)
                formatter.EndHeading();
            else
                formatter.EndParagraph();
            AddHTMLAfterParagraph();
        }
        protected override void AddHTMLBeforeParagraph()
        {
            formatter.StartSection();
        }
        private void HandleEditParagraphSpan()
        {
            if (formats.editable)
            {
                formatter.EndEditSpan(paragraphInfo);
            }
        }
        protected override void AddHTMLAfterParagraph()
        {
            if (!formats.sidenote)
            {
                formatter.EndSection();
                formatter.AppendClearDiv();
            }
        }
    }
}
