using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Myriad.Writer.Implementation;

namespace Myriad.Writer
{
    public static class WriterReference
    {
        public static HTMLWriter New()
        {
            return new HTMLStringWriter();
        }

        internal static HTMLWriter New(HttpResponse response)
        {
            return new HTMLResponseWriter(response);
        }
    }
}
