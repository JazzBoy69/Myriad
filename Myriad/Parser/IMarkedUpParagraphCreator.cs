namespace Myriad.Parser
{
    public interface IMarkedUpParagraphCreator
    {
        IMarkedUpParagraph Create(string paragraph);
    }

    public class MarkedUpParagraphCreator : IMarkedUpParagraphCreator
    {
        IMarkedUpParagraph IMarkedUpParagraphCreator.Create(string paragraph)
        {
            var markedupParagraph = new MarkedUpParagraph();
            markedupParagraph.Text = paragraph;
            return markedupParagraph;
        }
    }
}