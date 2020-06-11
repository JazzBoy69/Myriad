using System;
using Feliciana.Library;
using Microsoft.AspNetCore.Http;
using Myriad.Data;
using System.Collections.Generic;

namespace Myriad.Parser
{
    public enum ParagraphType
    {
        Article, Comment, Navigation, Chrono,
        Undefined
    }

    public class ParagraphInfo
    {
        public ParagraphType type;
        public int ID;
        public int index;

        public ParagraphInfo()
        {
            type = ParagraphType.Undefined;
        }

        public ParagraphInfo(HttpContext context)
        {
            context.Request.Form.TryGetValue("edittype", out var editTypeString);
            int editType = Convert.ToInt32(editTypeString);
            if (!Valid(editType)) return;
            type = (ParagraphType)editType;
            context.Request.Form.TryGetValue("ID", out var id);
            ID = Convert.ToInt32(id);
            context.Request.Form.TryGetValue("paragraphIndex", out var i);
            index = Convert.ToInt32(i);
        }

        public static Dictionary<ParagraphType, DataOperation> ReadOperations = new Dictionary<ParagraphType, DataOperation>()
        {
            { ParagraphType.Article, DataOperation.ReadArticleParagraph },
            { ParagraphType.Comment, DataOperation.ReadCommentParagraph },
            { ParagraphType.Navigation, DataOperation.ReadNavigationParagraph },
            { ParagraphType.Chrono, DataOperation.ReadChronoParagraph }
        };
        public static bool Valid(int paragraphType)
        {
            return (paragraphType >= Ordinals.first) && (paragraphType <= Ordinals.fourth);
        }

        public DataOperation ReadOperation { get { return ReadOperations[type]; } }
    }
}