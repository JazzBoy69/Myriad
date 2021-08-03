using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class ArticleParagraph
    {
        public int OriginalWord { get; set; } = 0;
        public int ParameterCount => 4;
        public int ID { get; private set; }
        public string Text { get; private set; }
        public int ParagraphIndex { get; private set; }
        public ArticleParagraph(int id, int index, string text) //three parameters fourth is original word
        {
            this.ID = id;
            this.ParagraphIndex = index;
            this.Text = CorrectEditString(text);
        }
        public static string CorrectEditString(string p)
        {
            p = " " + p;
            // p = p.Replace("\n", "");
            p = p.Replace(' ', ' ');
            p = p.Replace('‧', '∙');
            p = p.Replace('′', '΄');
            p = p.Replace('•', '∙');
            p = p.Replace('‧', '∙');
            p = p.Replace('´', '′');
            p = p.Replace("--", "—");
            p = p.Replace(". . . .", ". ._._.");
            p = p.Replace(". . .", "._._.");

            p = p.Replace(" \"", " “");
            p = p.Replace(" '", " ‘");

            p = p.Replace("“'", "“‘");



            p = p.Replace("—\"", "—“");
            p = p.Replace("—'", "—‘");

            p = p.Replace("(\"", "(“");
            p = p.Replace("('", "(‘");

            p = p.Replace("[\"", "[“");
            p = p.Replace("['", "[‘");

            p = p.Replace("^\"", "^“");
            p = p.Replace("**\"", "**“");
            p = p.Replace("//\"", "//“");
            p = p.Replace("^'", "^‘");
            p = p.Replace("**'", "**‘");
            p = p.Replace("//'", "//‘");

            p = p.Replace("'", "’");
            p = p.Replace(" \"", " “");
            p = p.Replace('"', '”');
            p = p.Trim();
            return p;
        }
    }
}
