using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public interface DataWriter<DataType> where DataType : DataObject
    {
        void WriteData(DataType data);

        void BeginTransaction();
        void Commit();
        void SetParameter<T>(int index, T value);
        void AddParameter<T>(int index, T value);

        void DeleteData<T>(T value);
    }
}
