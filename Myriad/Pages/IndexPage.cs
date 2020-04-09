using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Library;


namespace Myriad.Pages
{
    public struct IndexHTML
    {
        public const string IndexScripts = @"
<script>
   window.onload = function () {
    AddShortcut();
    SetupIndex();
SetupPartialPageLoad();
};
    </script>";
    }

    public class IndexPage : CommonPage
    {
        public const string pageURL = "/Index";
        public const string nameQuery = "name=";
        private const string requestNameQuery = "name";
        private const string defaultName = "home";
        List<string> paragraphs;
        int mainHeadingIndex;
        PageParser parser;
        string name = defaultName;
        int ID;
        public IndexPage() 
        {
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            name = (query.ContainsKey(requestNameQuery)) ? 
                query[requestNameQuery].ToString() :
                defaultName;
        }
        public async override Task RenderBody(HTMLWriter writer)
        {
            //todo edit page
            ID = GetPageID();
            paragraphs = GetPageParagraphs();
            parser = new PageParser(writer);
            parser.SetParagraphInfo(ParagraphType.Navigation, ID);
            await Parse();
            await AddPageTitleData(writer);
        }

        private async Task Parse()
        {
            bool foundFirstHeading = false;
            parser.SetStartHTML(HTMLTags.StartSectionWithClass + HTMLClasses.comment + HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndSection);
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[index].Length > Number.nothing) &&
                        (paragraphs[index][Ordinals.first] == '='))
                    {
                        await parser.ParseMainHeading(paragraphs[index]);
                        foundFirstHeading = true;
                        mainHeadingIndex = index;
                    }
                    continue;
                }
                await parser.ParseParagraph(paragraphs[index], index);
            }
            await parser.EndComments();
        }

        private int GetPageID()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationID), name);
            int result = reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        public List<string> GetPageParagraphs()
        {
            try
            {
                var reader = new DataReaderProvider<string>(
                    SqlServerInfo.GetCommand(DataOperation.ReadNavigationPage), name);
                var results = reader.GetData<string>();
                reader.Close();
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            try
            {
                var command = SqlServerInfo.GetCommand(DataOperation.ReadNavigationTitle);
                var reader = new DataReaderProvider<string>(command, name);
                await writer.Append(reader.GetDatum<string>());
                reader.Close();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected override string PageScripts()
        {
            return IndexHTML.IndexScripts;
        }

        public override string GetURL()
        {
            return pageURL;
        }

        public override bool IsValid()
        {
            return true;
        }

        public async override Task LoadTOCInfo()
        {
            await Task.Run(() =>
            {
                paragraphs = GetPageParagraphs();
                for (int index = Ordinals.first; index < paragraphs.Count; index++)
                {
                    if ((paragraphs[index].Length > Number.nothing) &&
                        (paragraphs[index][Ordinals.first] == '='))
                    {
                        mainHeadingIndex = index;
                        break;
                    }
                }
            });
        }
        public async override Task AddTOC(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartList);
            await writer.Append(HTMLTags.ID);
            await writer.Append(HTMLClasses.toc);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.visible);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationTitle), "");
            for (int index = Ordinals.first; index < mainHeadingIndex; index++)
            {
                reader.SetParameter(paragraphs[index]);
                string title = reader.GetDatum<string>();
                if (title == null) title = paragraphs[index];
                await writer.Append(HTMLTags.StartListItem);
                await writer.Append(HTMLTags.EndTag);
                await writer.Append(HTMLTags.StartAnchor);
                await writer.Append(HTMLTags.HREF);
                await writer.Append(pageURL);
                await writer.Append(HTMLTags.StartQuery);
                await writer.Append(nameQuery);
                await writer.Append(paragraphs[index]);
                await writer.Append(HTMLTags.Ampersand);
                await writer.Append(HTMLClasses.partial);
                await writer.Append(HTMLTags.OnClick);
                await writer.Append(JavaScriptFunctions.HandleTOCClick);
                await writer.Append(HTMLTags.EndTag);
                await writer.Append(title);
                await writer.Append(HTMLTags.EndAnchor);
                await writer.Append(HTMLTags.EndListItem);
            }
            reader.Close();
            await writer.Append(HTMLTags.EndList);
        }
    }
}
