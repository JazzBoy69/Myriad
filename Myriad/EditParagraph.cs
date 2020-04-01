using Microsoft.AspNetCore.Http;
using System;
using Myriad.Parser;
using Myriad.Data;

namespace Myriad
{
    internal class EditParagraph
    {
        internal static string getDataURL = "/EditParagraph/GetData";
        internal static void SendPlainText(HttpContext context)
        {
            context.Request.Form.TryGetValue("edittype", out var editType);
            ParagraphType paragraphType = (ParagraphType)Convert.ToInt32(editType);
            context.Request.Form.TryGetValue("ID", out var ID);
            int articleID = Convert.ToInt32(ID);
            context.Request.Form.TryGetValue("paragraphIndex", out var index);
            int paragraphIndex = Convert.ToInt32(index);
            switch (paragraphType)
            {
                case ParagraphType.Article:
                    SendPlainTextParagraph(DataOperation.ReadArticleParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Comment:
                    SendPlainTextParagraph(DataOperation.ReadCommentParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Navigation:
                    SendPlainTextParagraph(DataOperation.ReadNavigationParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Undefined:
                    break;
                default:
                    break;
            }
        }

        private static void SendPlainTextParagraph(DataOperation operation, int articleID, int paragraphIndex, HttpResponse response)
        {
            var reader = SQLServerReaderProvider<int, int>.Reader(operation, articleID, paragraphIndex);
            response.WriteAsync(reader.GetDatum<string>());
        }
    }
}