﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data.Implementation;

namespace Myriad.Data
{
    public enum DataOperation { ReadNavigationPage }
    public interface DataReader
    {
        public abstract List<T> GetData<T>(DataOperation operation, string key);
    }

    public static class ReaderProvider
    {
        public static DataReader Reader()
        {
            return new SqlServerDataReader();
        }
    }

}
