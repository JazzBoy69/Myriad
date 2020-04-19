using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Pages
{
    public abstract class PaginationPage : CommonPage
    {
        public abstract Task SetupNextPage();
        public abstract Task SetupPrecedingPage();
    }
}
