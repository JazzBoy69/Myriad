using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data.Implementation;

namespace Myriad.Data
{
    public enum DataOperation { ReadNavigationPage, ReadArticleTitle, ReadArticleID, 
        ReadArticle, ReadCommentIDs, ReadCommentLinks, ReadCommentParagraphs,
        ReadKeywords
    }
    public static class DataObjectFactory<DataObjectType> where DataObjectType:DataObject, new()
    {
        public static DataObjectType Read(DbDataReader reader)
        {
            DataObjectType dataObject = new DataObjectType();
            dataObject.Read(reader);
            return dataObject;
        }
    }

    public interface DataObject
    {
        void Read(DbDataReader reader);
    }
    public interface DataReader<KeyType>
    {
        List<DataType> GetData<DataType>();
        DataType GetDatum<DataType>();
        List<ValueTuple<T1, T2>> GetData<T1, T2>();

        List<ClassType> GetClassData<ClassType>() where ClassType : DataObject, new();
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
