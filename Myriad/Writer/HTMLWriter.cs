using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad
{
    public interface HTMLWriter
    {
        public void StartSpanWithClass(string className);
        public void StartAnchorWithClass(string className);
        public void StartDivWithClass(string className);

        public void StartSectionWithClass(string className);

        public void StartDivWithID(string id);
        public void StartFigure(string className);
        public void StartIMG(string path);
        public void Append(char c);

        public void Append(ReadOnlySpan<char> span);
        public void Append(string stringToAppend);
        public void Append(int number);
        public void AppendHREF(string pageName);
        public void AppendIMGWidth(string widthString);
        public void AppendClass(string className);
        public string Response();
        void StartParagraphWithClass(string className);
    }
}
