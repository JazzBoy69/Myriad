using Myriad.Data.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

namespace Myriad.Data
{
    public interface DataObject
    {
        int ParameterCount { get; }

        void Read(DbDataReader reader);
        void Create(DbCommand command);
        void AddParameterTo<DataType>(DataWriter<DataType> writer, int index) where DataType : DataObject;
    }
}
