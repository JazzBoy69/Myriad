using System;
using Feliciana.Library;
using Microsoft.AspNetCore.Http;
using Myriad.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        static readonly Dictionary<ParagraphType, Func<int, int, Task<string>>> readMethods =
        new Dictionary<ParagraphType, Func<int, int, Task>>()
        {
                {ParagraphType.Article, DataRepository.GlossaryParagraph },
                {ParagraphType.Comment, DataRepository.CommentParagraph },
                {ParagraphType.Navigation, DataRepository.NavigationParagraph },
                {ParagraphType.Chrono, DataRepository.CommentChapterParagraph }
        };

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
        public static bool Valid(int paragraphType)
        {
            return (paragraphType >= Ordinals.first) && (paragraphType <= Ordinals.fourth);
        }
    }
}