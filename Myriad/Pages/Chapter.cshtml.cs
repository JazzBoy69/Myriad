﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Myriad
{
    public class ChapterModel
    {
        public const string pageURL = "/Chapter";
        public const string queryKeyStartVerse = "startverse=";
        public const string queryKeyEndVerse = "endverse=";
        public void OnGet()
        {

        }
    }
}