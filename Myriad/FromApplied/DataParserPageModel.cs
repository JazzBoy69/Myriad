
using Microsoft.AspNetCore.Mvc.RazorPages;
using Myriad.AppliedClasses;
using Myriad.ToApplied;


namespace Myriad.FromApplied
{
    public class DataParserPageModel : PageModel
    {
        public MarkupParser GetMarkupParser()
        {
            return new AppliedMarkupParser();
        }

        public DataReader GetDataReader(string selectionString)
        {
            return new AppliedDataReader(selectionString) as DataReader;
        }
    }
}