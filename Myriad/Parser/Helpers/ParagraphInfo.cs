namespace Myriad.Parser
{
    public enum ParagraphType
    {
        Article, Comment, Navigation, Chrono,
        Undefined
    }

    public struct ParagraphInfo
    {
        public ParagraphType type;
        public int ID;
        public int index;
    }
}