using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    enum DataOperation { ReadNavigationPage }
    public class DataReader
    {
        internal static object GetData(DataOperation operation, string key)
        {
            throw new NotImplementedException();
        }
    }


    public class IDataResultList<T> 
    {
        List<T> result = new List<T>();
        public List<T> Result { get { return result; } }

        public int Count
        {
            get
            {
                return result.Count;
            }
        }

        public void Add(T item)
        {
            result.Add(item);
        }
    }
}
