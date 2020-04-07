using Myriad.Paragraph.Implementation;
//todo generic paragraph and string range in library
namespace Myriad.Paragraph
{
    public class ParagraphReference
    {
        public static IMarkedUpParagraph New(string s)
        {
            var paragraph = new MarkedUpParagraph
            {
                Text = s
            };
            return paragraph;
        }
    }
}
