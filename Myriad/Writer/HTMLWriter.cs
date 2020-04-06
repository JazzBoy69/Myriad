using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad
{
    public interface HTMLWriter
    {
        public Task StartSpanWithClass(string className);
        public Task StartAnchorWithClass(string className);
        public Task StartDivWithClass(string className);

        public Task StartSectionWithClass(string className);

        public Task StartDivWithID(string id);
        public Task StartFigure(string className);
        public Task StartIMG(string path);
        public Task Append(char c);
        public Task Append(string stringToAppend);
        public Task Append(int number);
        public Task AppendHREF(string pageName);
        public Task AppendIMGWidth(string widthString);
        public Task AppendClass(string className);
        public string Response();
        public Task StartParagraphWithClass(string className);
    }
}
