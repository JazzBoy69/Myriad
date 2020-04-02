using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data.Implementation;

namespace Myriad.Data
{
    public enum DataOperation { ReadNavigationPage, ReadNavigationParagraph, ReadNavigationID, 
        ReadArticleTitle, ReadArticleID, ReadArticle, ReadArticleParagraph,
        ReadCommentIDs, ReadCommentLinks, ReadComment, ReadCommentParagraph,
        ReadKeywords, ReadImageSize,

            CreateNavigationParagraph=256, UpdateNavigationParagraph=257, DeleteNavigationParagraph=258,
            CreateArticleParagraph=270, UpdateArticleParagraph=271, DeleteArticleParagraph=272,
            CreateCommentParagraph=280, UpdateCommentParagraph=281, DeleteCommentParagraph=282

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

    public interface DataReader<KeyType>
    {
        List<DataType> GetData<DataType>();
        DataType GetDatum<DataType>();
        List<ValueTuple<T1, T2>> GetData<T1, T2>();

        List<ClassType> GetClassData<ClassType>() where ClassType : DataObject, new();

        ClassType GetClassDatum<ClassType>() where ClassType : DataObject, new();
        void Open();
    }

    public interface DataReader<KeyType1, KeyType2> : DataReader<KeyType1>
    {
    }

    public static class SQLServerReaderProvider<KeyType>
    {
        public static DataReader<KeyType> Reader(DataOperation operation, KeyType key)
        {
            return new SqlServerDataReader<KeyType>(operation, key);
        }
    }

    public static class SQLServerWriterProvider<DataType> where DataType : DataObject
    {
        public static DataWriter<DataType> Writer(DataOperation operation)
        {
            return new SqlServerDataWriter<DataType>(operation);
        }
    }

    public static class SQLServerReaderProvider<KeyType1, KeyType2>
    {
        public static DataReader<KeyType1, KeyType2> Reader(DataOperation operation, 
            KeyType1 key1, KeyType2 key2)
        {
            return new SqlServerDataReader<KeyType1, KeyType2>(operation, key1, key2);
        }
    }
}
