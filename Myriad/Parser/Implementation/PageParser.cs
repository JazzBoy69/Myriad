using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class PageParser : MarkupParser
    {
        public PageParser(HTMLWriter builder) : base(builder)
        {
        }

        protected override async Task HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            await AddHTMLBeforeParagraph();
            if (currentParagraph.Length > 1)
            {
                foundEndToken = await HandleStartToken();
            }
            if (!formats.heading && !formats.figure)
            {
                await formatter.Append(HTMLTags.StartParagraph);
                if (formats.editable)
                {
                    await formatter.StartEditSpan(paragraphInfo);
                }
            }
        }

        protected override async Task HandleEnd()
        {
            mainRange.MoveEndToLimit();
            if (citationLevel > 0)
            {
                mainRange.MoveStartTo(lastDash + 1);
                mainRange.PullEnd();
                await HandleLastDashCitations();
                mainRange.BumpEnd();
            }
            await formatter.AppendString(currentParagraph, mainRange);

            await HandleEditParagraphSpan();

            if (formats.heading)
                await formatter.Append(HTMLTags.EndHeader);
            else
                await formatter.Append(HTMLTags.EndParagraph);
            await AddHTMLAfterParagraph();
        }
        private async Task HandleEditParagraphSpan()
        {
            if (formats.editable)
            {
                await formatter.EndEditSpan(paragraphInfo);
            }
        }
        protected override async Task AddHTMLAfterParagraph()
        {
            if (!formats.sidenote)
            {
                await formatter.Append(HTMLTags.EndSection);
                await formatter.AppendClearDiv();
            }
        }

        internal async Task EndComments()
        {
            await formatter.Append(HTMLTags.EndDiv);
        }

    }
}
