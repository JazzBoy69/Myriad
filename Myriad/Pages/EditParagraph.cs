using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Parser;
using Myriad.Data;
using Microsoft.Extensions.Primitives;

namespace Myriad.Pages
{
    internal class EditParagraph
    {
        internal static string getDataURL = "/EditParagraph/GetData";
        internal static string setDataURL = "/EditParagraph/SetData";
        internal static async Task GetPlainText(HttpContext context)
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
                    await SendPlainTextParagraph(DataOperation.ReadArticleParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Comment:
                    await SendPlainTextParagraph(DataOperation.ReadCommentParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Navigation:
                    await SendPlainTextParagraph(DataOperation.ReadNavigationParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Undefined:
                    break;
                default:
                    break;
            }
        }

        private static async Task SendPlainTextParagraph(DataOperation operation, int articleID, int paragraphIndex, HttpResponse response)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(operation), articleID, paragraphIndex);
            await response.WriteAsync(reader.GetDatum<string>());
        }

        internal static async Task SetText(HttpContext context)
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
            DataWriterProvider.WriteData(SqlServerInfo.GetCommand(writeOperation),
                articleParagraph);
            MarkupParser parser = new MarkupParser(Writer.New(context.Response));
            await parser.ParseParagraph(text, paragraphIndex);
        }
    }
}