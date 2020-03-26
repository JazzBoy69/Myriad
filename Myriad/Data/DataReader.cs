using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data.Implementation;

namespace Myriad.Data
{
    public enum DataOperation { ReadNavigationPage, ReadArticleTitle, ReadArticleID, 
        ReadArticle, ReadCommentIDs, ReadCommentLinks, ReadCommentParagraphs
    }
    public interface DataReader<KeyType>
    {
        List<DataType> GetData<DataType>();
        DataType GetDatum<DataType>();
        List<ValueTuple<T1, T2>> GetData<T1, T2>();
    }

    public interface DataReader<KeyType1, KeyType2> : DataReader<KeyType1>
    {
    }

    public static class ReaderProvider<KeyType>
    {
        public static DataReader<KeyType> Reader(DataOperation operation, KeyType key)
        {
            return new SqlServerDataReader<KeyType>(operation, key);
        }
    }

    public static class ReaderProvider<KeyType1, KeyType2>
    {
        public static DataReader<KeyType1, KeyType2> Reader(DataOperation operation, 
            KeyType1 key1, KeyType2 key2)
        {
            return new SqlServerDataReader<KeyType1, KeyType2>(operation, key1, key2);
        }
    }

}
