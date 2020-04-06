using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad
{
    public interface HTMLWriter
    {
        public Task Append(char c);
        public Task Append(string stringToAppend);
        public Task Append(int number);
        public string Response();
    }
}
