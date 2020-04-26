using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;

namespace Myriad.Parser

{
    public struct Tokens
    {
        public const int headingToken = 0x3D3D;
        public const int startSidenote = 0x2828;
        public const int endSidenote = 0x2929;
        public const int picture = 0x5B5B;
        public const int bold = 0x2A2A;
        public const int italic = 0x2F2F;
        public const int detail = 0x2B2B;
        public static readonly char[] brackettokens = new char[] { '|', '}' };
        public static readonly char[] tokens = new char[] { '*', '^', '/', '=', '(', '[', '{', ')', ']', '}', '~', '#', '|', '_', '+' };
    }
    public interface IParser
    {
        abstract public void SetStartHTML(string html);
        abstract public void SetEndHTML(string html);
        abstract public Task ParseParagraph(string paragraph, int index);

        IParagraph CurrentParagraph { get; }

        StringRange MainRange { get; }
       
        abstract void SearchForToken();
        abstract Task HandleToken();
        abstract int DecreaseCitationLevel();
        abstract Task HandleCitations();

    }
}