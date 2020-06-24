using System;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;

namespace Myriad.Parser
{
    public class PageParser : MarkupParser
    {
        public PageParser(HTMLWriter writer) : base(writer)
        {
        }

        protected override async Task HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            formats.hideDetails = hideDetails;
            if (currentParagraph.Length > 1)
            {
                foundEndToken = await HandleTableTokens();
                if (foundEndToken) return;
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

        internal async Task<bool> HandleTableTokens()
        {
            int token = currentParagraph.TokenAt(Ordinals.first);
            if (token == Tokens.startTable)
            {
                await ParseTableHeader(currentParagraph.Text.Substring(Ordinals.third));
                return true;
            }
            if (token == Tokens.endTable)
            {
                await formatter.Append(HTMLTags.EndTable);
                return true;
            }
            if (token == Tokens.tableRow)
            {
                await ParseTableRow(currentParagraph.Text.Substring(Ordinals.third));
                return true;
            }
            return false;
        }
        private async Task ParseTableHeader(string header)
        {
            string[] cells = header.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            await formatter.Append("<table><tr>");
            foreach (string cell in cells)
            {
                if (cell.Length < 2)
                {
                    await formatter.Append("<th></th>");
                    mainRange.MoveEndToLimit();
                    mainRange.MoveStartTo(mainRange.End + 2);
                    continue;
                }
                bool left = cell.First() != ' ';
                bool right = cell.Last() != ' ';

                if (!left && !right)
                {
                    await formatter.Append("<th class='center'>");
                }
                else
                if (right)
                {
                    await formatter.Append("<th class='right'>");
                }
                else
                {
                    await formatter.Append("th class='left'>");
                }
                await ParseString(cell.Trim());
                await formatter.Append("</th>");
            }
            await formatter.Append("</tr>");
        }

        internal async Task ParseTableRow(string row)
        {
            string[] cells = row.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            await formatter.Append("<tr>");
            foreach (string cell in cells)
            {
                if (cell.Length < 2)
                {
                    await formatter.Append("<td></td>");
                    continue;
                }
                bool left = cell.First() != ' ';
                bool right = cell.Last() != ' ';
                if (!left && !right)
                {
                    await formatter.Append("<td class='center'>");
                }
                else
                if (right)
                {
                    await formatter.Append("<td class='right'>");
                }
                else
                {
                    await formatter.Append("td class='left'>");
                }
                await ParseString(cell.Trim());
                await formatter.Append("</td>");
            }
            await formatter.Append("</tr>");
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
