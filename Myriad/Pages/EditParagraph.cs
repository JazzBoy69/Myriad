using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;
using Myriad.Parser;
using Myriad.Data;
using Microsoft.Extensions.Primitives;

namespace Myriad.Pages
{
    internal class EditParagraph
    {
        internal static string getDataURL = "/EditParagraph/GetData";
        internal static string setDataURL = "/EditParagraph/SetData";
        internal static void GetPlainText(HttpContext context)
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
            var reader = DataReaderProvider<int, int>.Reader(operation, articleID, paragraphIndex);
            response.WriteAsync(reader.GetDatum<string>());
        }

        internal static void SetText(HttpContext context)
        {
            context.Request.Form.TryGetValue("edittype", out var editType);
            ParagraphType paragraphType = (ParagraphType)Convert.ToInt32(editType);
            context.Request.Form.TryGetValue("ID", out var ID);
            int articleID = Convert.ToInt32(ID);
            context.Request.Form.TryGetValue("paragraphIndex", out var index);
            int paragraphIndex = Convert.ToInt32(index);
            context.Request.Form.TryGetValue("text", out var text);
            DataOperation writeOperation;
            switch (paragraphType)
            {
                case ParagraphType.Article:
                    writeOperation = DataOperation.UpdateArticleParagraph;
                    break;
                case ParagraphType.Comment:
                    writeOperation = DataOperation.UpdateCommentParagraph;
                    break;
                case ParagraphType.Navigation:
                    writeOperation = DataOperation.UpdateNavigationParagraph;
                    break;
                default:
                    return;
            }
            ArticleParagraph articleParagraph = new ArticleParagraph(articleID, paragraphIndex, text);
            var articleWriter = DataWriterProvider<ArticleParagraph>
                .Writer(writeOperation);
            articleWriter.BeginTransaction();
            articleWriter.WriteData(articleParagraph);
            articleWriter.Commit();
            MarkupParser parser = new MarkupParser(Writer.New(context.Response));
            parser.ParseParagraph(text, paragraphIndex);
        }
    }
}