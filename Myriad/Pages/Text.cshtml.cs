using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Myriad
{
    public class TextModel
    {
        public const string queryKeyTGStart = "tgstart=";
        public const string queryKeyTGEnd = "tgend=";
        public const string queryKeyStartVerse = "startverse=";
        public const string queryKeyEndVerse = "endverse=";
        public const string pageURL = "/Text";
        public void OnGet()
        {

        }
    }
}