using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class ArticleParser : MarkupParser
    {
        int ID;
        public ArticleParser(HTMLResponse builder, int articleID) : base(builder)
        {
            ID = articleID;
        }

        public override void Parse(List<string> paragraphs)
        {
            bool foundFirstHeading = false;
            SetParagraphInfo(ParagraphType.Article, ID);
            for (int i = Ordinals.first; i<paragraphs.Count; i++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[i].Length > Numbers.nothing) &&
                        (paragraphs[i][Ordinals.first] == '='))
                    {
                        currentParagraph = creator.Create(paragraphs[i]);
                        ParseMainHeading();

                        foundFirstHeading = true;
                    }
                    continue;
                }
                currentParagraph = creator.Create(paragraphs[i]);
                paragraphInfo.index = i;
                ParseParagraph();
            }
            EndComments();
        }
    }
}

