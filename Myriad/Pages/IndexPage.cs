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

        List<string> paragraphs;
        int mainHeadingIndex;
        PageParser parser;
        HTMLWriter writer;
        int ID;
        public IndexPage() 
        {
        }

        public async override Task RenderBody(HTMLWriter writer)
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
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationID, "home");
            int result = reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        public List<string> GetPageParagraphs()
        {
            var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationPage, "home");
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }

        protected override string GetTitle()
        {
            return "Myriad Study Bible";
        }

        protected override string PageScripts()
        {
            return IndexHTML.IndexScripts;
        }

        public override string GetURL()
        {
            return pageURL;
        }

        public override void LoadQueryInfo(IQueryCollection query)
        {
        }

        public override bool IsValid()
        {
            return true;
        }

        public async override Task AddTOC()
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartList);
                writer.Append(HTMLTags.ID);
                writer.Append(HTMLClasses.toc);
                writer.Append(HTMLTags.Class);
                writer.Append(HTMLClasses.hidden);
                writer.Append(HTMLTags.CloseQuoteEndTag);
                var reader = DataReaderProvider<string>.Reader(DataOperation.ReadNavigationTitle, "");
                for (int index = Ordinals.first; index < mainHeadingIndex; index++)
                {
                    reader.SetParameter(paragraphs[index]);
                    string title = reader.GetDatum<string>();
                    writer.Append(HTMLTags.StartListItem);
                    writer.Append(HTMLTags.EndTag);
                    writer.Append(HTMLTags.StartAnchor);
                    writer.Append(HTMLTags.HREF);
                    writer.Append(pageURL);
                    writer.Append(HTMLTags.StartQuery);
                    writer.Append(nameQuery);
                    writer.Append(paragraphs[index]);
                    writer.Append(HTMLTags.EndTag);
                    writer.Append(title);
                    writer.Append(HTMLTags.EndAnchor);
                    writer.Append(HTMLTags.EndListItem);
                }
                reader.Close();
                writer.Append(HTMLTags.EndList);
            });
        }
    }
}
