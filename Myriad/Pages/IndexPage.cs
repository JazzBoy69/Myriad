using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
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
        HTMLWriter writer;
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
            try
            {
                //todo edit page
                this.writer = writer;
                ID = GetPageID();
                paragraphs = GetPageParagraphs();
                parser = new PageParser(writer);
                parser.SetParagraphInfo(ParagraphType.Navigation, ID);
                Parse();
                await AddPageTitleData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Parse()
        {
            bool foundFirstHeading = false;
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[index].Length > Number.nothing) &&
                        (paragraphs[index][Ordinals.first] == '='))
                    {
                        parser.ParseMainHeading(paragraphs[index]);
                        foundFirstHeading = true;
                        mainHeadingIndex = index;
                    }
                    continue;
                }
                parser.ParseParagraph(paragraphs[index], index);
            }
            parser.EndComments();
        }

        private int GetPageID()
        {
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationID, name);
            int result = reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        public List<string> GetPageParagraphs()
        {
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationPage, name);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }

        protected override string GetTitle()
        {
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationTitle, name);
            var result = reader.GetDatum<string>();
            reader.Close();
            return result;
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
            await Write(HTMLTags.StartList);
            await Write(HTMLTags.ID);
            await Write(HTMLClasses.toc);
            await Write(HTMLTags.Class);
            await Write(HTMLClasses.visible);
            await Write(HTMLTags.CloseQuoteEndTag);
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationTitle, "");
            for (int index = Ordinals.first; index < mainHeadingIndex; index++)
            {
                reader.SetParameter(paragraphs[index]);
                string title = reader.GetDatum<string>();
                if (title == null) title = paragraphs[index];
                await Write(HTMLTags.StartListItem);
                await Write(HTMLTags.EndTag);
                await Write(HTMLTags.StartAnchor);
                await Write(HTMLTags.HREF);
                await Write(pageURL);
                await Write(HTMLTags.StartQuery);
                await Write(nameQuery);
                await Write(paragraphs[index]);
                await Write(HTMLTags.Ampersand);
                await Write(HTMLClasses.partial);
                await Write(HTMLTags.OnClick);
                await Write(JavaScriptFunctions.HandleTOCClick);
                await Write(HTMLTags.EndTag);
                await Write(title);
                await Write(HTMLTags.EndAnchor);
                await Write(HTMLTags.EndListItem);
            }
            reader.Close();
            await Write(HTMLTags.EndList);
        }
    }
}
