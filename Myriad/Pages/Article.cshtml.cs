using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Myriad
{
    public class ArticleModel : PageModel
    {
        public const string pageURL = "/Article";
        public const string queryKeyTitle = "title=";
        public void OnGet()
        {

        }
    }
}