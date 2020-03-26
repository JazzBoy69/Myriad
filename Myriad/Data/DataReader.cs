using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data.Implementation;

namespace Myriad.Data
{
    public enum DataOperation { ReadNavigationPage, ReadArticleTitle, ReadArticleID, 
        ReadArticle, ReadCommentIDs }
    public interface DataReader
    {
        public abstract List<T> GetData<T>(DataOperation operation, string key);
        T GetDatum<T>(DataOperation operation, int key);
        T GetDatum<T>(DataOperation operation, string key);
        List<T> GetData<T>(DataOperation readCommentIDs, int startID, int endID);
    }

    public static class ReaderProvider
    {
        public static DataReader Reader()
        {
            return new SqlServerDataReader();
        }
    }

}
