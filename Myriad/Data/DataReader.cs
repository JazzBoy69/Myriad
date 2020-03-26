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
    public interface DataReader
    {
        List<T> GetData<T>(DataOperation operation, object key);
        T GetDatum<T>(DataOperation operation, object key);
        List<T> GetData<T>(DataOperation operation, object key1, object key2);
        List<ValueTuple<T1, T2>> GetData<T1, T2>(DataOperation operation, object key1, object key2);
        List<ValueTuple<T1, T2>> GetData<T1, T2>(DataOperation operation, object key);
    }

    public static class ReaderProvider
    {
        public static DataReader Reader()
        {
            return new SqlServerDataReader();
        }
    }

}
