using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Myriad
{
    public class VerseModel
    {
        public const string pageURL = "/Verse";
        public const string queryKeyVerse = "verse=";
        public void OnGet()
        {

        }
    }
}