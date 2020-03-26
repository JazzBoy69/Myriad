using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Myriad.Library;

namespace Myriad.Data.Implementation
{
    public class SqlServerDataReader : DataReader
    {
        static Dictionary<DataOperation, string> selectors = new Dictionary<DataOperation, string>()
        {
            { DataOperation.ReadNavigationPage,
                "select text from navigationparagraphs where name=@key order by paragraphindex" },
            { DataOperation.ReadArticleTitle,
                "select title from tags where id=@key"},
            { DataOperation.ReadArticleID,
                "select id from tags where title=@key" },
            { DataOperation.ReadArticle,
                "select text from glossary where id=@key" },
            { DataOperation.ReadCommentIDs,
                "select id from commentlinks where last>= @key1 and start<=@key2" },
            { DataOperation.ReadCommentParagraphs,
                "select text from comments where id=@key" }
        };
        private static string connectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";

        public List<T> GetData<T>(DataOperation operation, object key)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key", key);
            using var reader = command.ExecuteReader();
            List<T> results = new List<T>();
            while (reader.Read())
            {
                results.Add((T)reader.GetValue(Ordinals.first));
            }
            connection.Close();
            return results;
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public T GetDatum<T>(DataOperation operation, object key)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key", key);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (T)reader.GetValue(Ordinals.first);
            }
            connection.Close();
            return default;
        }
        public T GetDatum<T>(DataOperation operation, int key)
        {
            return GetDatum<T>(operation, key.ToString());
        }

        public List<T> GetData<T>(DataOperation operation, object key1, object key2)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key1", key1);
            command.Parameters.AddWithValue("@key2", key2);
            using var reader = command.ExecuteReader();
            List<T> results = new List<T>();
            while (reader.Read())
            {
                results.Add((T)reader.GetValue(Ordinals.first));
            }
            connection.Close();
            return results;
        }

        public List<(T1, T2)> GetData<T1, T2>(DataOperation operation, object key1, object key2)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key1", key1);
            command.Parameters.AddWithValue("@key2", key2);
            using var reader = command.ExecuteReader();
            List<(T1, T2)> results = new List<(T1, T2)>();
            while (reader.Read())
            {
                results.Add(((T1)reader.GetValue(Ordinals.first),
                    (T2)reader.GetValue(Ordinals.second)));
            }
            connection.Close();
            return results;
        }

        public List<(T1, T2)> GetData<T1, T2>(DataOperation operation, object key)
        {
            using var connection = GetConnection();
            connection.Open();
            using var command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key", key);
            using var reader = command.ExecuteReader();
            List<(T1, T2)> results = new List<(T1, T2)>();
            if (reader.Read())
            {
                results.Add(((T1)reader.GetValue(Ordinals.first),
                    (T2)reader.GetValue(Ordinals.second)));
            }
            connection.Close();
            return results;
        }
    }
}
