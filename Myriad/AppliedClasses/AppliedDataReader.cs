using System.Collections.Generic;
using Myriad.ToApplied;
using Myriad.Library;

namespace Myriad.AppliedClasses
{
    internal class AppliedDataReader : DataReader
    {
        string selectionString;
        internal AppliedDataReader(string selectionString)
        {
            this.selectionString = selectionString;
        }

        List<string> DataReader.GetData(string key)
        {
            DataConnectionLiason connectionLiason = new DataConnectionLiason();
            DataCommandLiason commandLiason = new DataCommandLiason(selectionString);
            commandLiason.Command.Parameters.AddWithValue("@key", key);
            commandLiason.Command.Connection = connectionLiason.Connection;
            DataReaderLiason readerLiason = new DataReaderLiason(commandLiason);
            List<string> result = new List<string>();
            while (readerLiason.Reader.Read())
            {
                result.Add(readerLiason.Reader.GetString(Ordinals.first));
            }
            return result;
        }
    }

}