using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Writer.Implementation
{
    //todo move to writer library
    public class HTMLResponseWriter : HTMLWriter
    {
        private readonly HttpResponse response;

        public HTMLResponseWriter(HttpResponse response)
        {
            this.response = response;
        }

        public async Task Append(char c)
        {
            await response.WriteAsync(c.ToString());
        }

        public async Task Append(string stringToAppend)
        {
            await response.WriteAsync(stringToAppend);
        }

        public async Task Append(int number)
        {
            await response.WriteAsync(number.ToString());
        }

        public string Response()
        {
            return "";
        }

    }
}