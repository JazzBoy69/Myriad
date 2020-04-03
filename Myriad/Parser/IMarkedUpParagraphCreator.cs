namespace Myriad.Parser
{
    public enum ParagraphType
    {
        Article, Comment, Navigation,
        Undefined
    }

    public struct ParagraphInfo
    {
        public ParagraphType type;
        public int ID;
        public int index;
    }

    public interface IMarkedUpParagraphCreator
    {
        IMarkedUpParagraph Create(string paragraph);
    }

    public class MarkedUpParagraphCreator : IMarkedUpParagraphCreator
    {
        //todo should be able to eliminate this. Only one kind of markedup paragraph
        IMarkedUpParagraph IMarkedUpParagraphCreator.Create(string paragraph)
        {
            var markedupParagraph = new MarkedUpParagraph();
            markedupParagraph.Text = paragraph;
            return markedupParagraph;
        }
    }
}