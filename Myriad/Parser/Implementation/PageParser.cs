using System.Threading.Tasks;
using Feliciana.HTML;
using Feliciana.ResponseWriter;

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
            if (currentParagraph.Length > 1)
            {
                foundEndToken = await HandleStartToken();
            }
            if (!formats.heading && !formats.figure)
            {
                await AddHTMLBeforeParagraph();
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
                await HandleCitations();
            } 
            await formatter.AppendString(currentParagraph, mainRange);
            await HandleEditParagraphSpan();
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
            if (formats.heading)
            {
                await formatter.Append(HTMLTags.EndHeader);
                return;
            }
            if (!formats.sidenote)
            {
                await formatter.Append(endHTML);
                await formatter.AppendClearDiv();
            }
        }

        internal async Task EndComments()
        {
            await formatter.Append(HTMLTags.EndDiv);
        }

    }
}
