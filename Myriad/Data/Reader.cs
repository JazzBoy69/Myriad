using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Data;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.Data
{
    public class Reader
    {
        public static List<RubyInfo> ReadSustituteText(int id)
        {
            var reader = new StoredProcedureProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSubstituteWords),
                id);
            List<RubyInfo> substituteWords = reader.GetClassData<RubyInfo>();
            reader.Close();
            return substituteWords;
        }
    }
}
