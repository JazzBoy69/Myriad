using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Myriad.Library;

namespace Myriad.Data.Implementation
{
    public class SqlServerInfo
    {
        internal const string key1 = "@key1";
        internal const string key2 = "@key2";
        internal static Dictionary<DataOperation, string> Selectors = new Dictionary<DataOperation, string>()
        {
            { DataOperation.ReadNavigationPage,
                "select text from navigationparagraphs where name=@key1 order by paragraphindex" },
            { DataOperation.ReadArticleTitle,
                "select title from tags where id="+ key1},
            { DataOperation.ReadArticleID,
                "select id from tags where title="+ key1 },
            { DataOperation.ReadArticle,
                "select text from glossary where id="+ key1 },
            { DataOperation.ReadCommentIDs,
                "select id from commentlinks where last>= "+ key1+" and start<="+key2 },
            { DataOperation.ReadCommentParagraphs,
                "select RTrim(text) from comments where id="+ key1 },
            { DataOperation.ReadCommentLinks,
                "select start, last from commentlinks where id="+ key1 },
            { DataOperation.ReadKeywords,
                "select keyid, RTrim(leadingsymbols), RTrim(text), RTrim(trailingsymbols)+' ', iscapitalized, poetic, sentence*256+sentencewordindex from keywords"+
                " where keyid>="
                + key1 + " and keyid<=" + key2 }
        };
        internal static SqlConnection Connection()
        {
            return new SqlConnection(ConnectionString);
        }
        static string ConnectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";
    }
    public class SqlServerDataReader<KeyType> : DataReader<KeyType>
    {
        protected SqlDataReader reader;
        protected SqlConnection connection;
        protected SqlCommand command;
        public SqlServerDataReader(DataOperation operation, KeyType key)
        {
            connection = SqlServerInfo.Connection();
            connection.Open();
            command = new SqlCommand(SqlServerInfo.Selectors[operation], connection);
            command.Parameters.AddWithValue(SqlServerInfo.key1, key);
        }

        public List<DataType> GetData<DataType>()
        {
            reader = command.ExecuteReader();
            List<DataType> results = new List<DataType>();
            while (reader.Read())
            {
                results.Add(reader.GetFieldValue<DataType>(Ordinals.first));
            }
            Close();
            return results;
        }

        private void Close()
        {
            reader.Close();
            command.Dispose();
            connection.Close();
        }

        public List<(T1, T2)> GetData<T1, T2>()
        {
            reader = command.ExecuteReader();
            List<(T1, T2)> results = new List<(T1, T2)>();
            while (reader.Read())
            {
                results.Add((reader.GetFieldValue<T1>(Ordinals.first),
                    reader.GetFieldValue<T2>(Ordinals.second)));
            }
            Close();
            return results;
        }

        public DataType GetDatum<DataType>()
        {
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                DataType result = reader.GetFieldValue<DataType>(Ordinals.first);
                Close();
                return result;
            }
            Close();
            return default;
        }

        public List<ClassType> GetClassData<ClassType>() where ClassType : DataObject, new()
        {
            reader = command.ExecuteReader();
            List<ClassType> results = new List<ClassType>();
            while (reader.Read())
            {
                results.Add(DataObjectFactory<ClassType>.Read(reader));
            }
            Close();
            return results;
        }
    }

    public class SqlServerDataReader<KeyType1, KeyType2> : SqlServerDataReader<KeyType1>, DataReader<KeyType1,KeyType2> 
    {
        public SqlServerDataReader(DataOperation operation, KeyType1 key1, KeyType2 key2) : base(operation, key1)    
        {
            command.Parameters.AddWithValue(SqlServerInfo.key2, key2);
        }
    }
}
