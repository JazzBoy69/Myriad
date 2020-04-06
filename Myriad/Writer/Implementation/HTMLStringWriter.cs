using System;
using System.Text;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Writer.Implementation
{
    public class HTMLStringWriter : HTMLWriter
    {
        readonly StringBuilder writer = new StringBuilder();
        private StringBuilder Writer => writer;

        public string Response()
        {
            return Writer.ToString();
        }

        public async Task Append(char c)
        {
            await Task.Run(() => writer.Append(c));
        }

        public async Task Append(string stringToAppend)
        {
            await Task.Run(() => writer.Append(stringToAppend));
        }

        public async Task Append(int number)
        {
            await Task.Run(() => writer.Append(number));
        }

    }
}