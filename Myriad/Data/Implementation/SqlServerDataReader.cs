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
                "select text from navigationparagraphs where name=@key order by paragraphindex" }
        };
        private static string connectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";

        public List<T> GetData<T>(DataOperation operation, string key)
        {
            SqlConnection connection = GetConnection();
            connection.Open();
            SqlCommand command = new SqlCommand(selectors[operation], connection);
            command.Parameters.AddWithValue("@key", key);
            SqlDataReader reader = command.ExecuteReader();
            List<T> results = new List<T>();
            while (reader.Read())
            {
                results.Add((T)reader.GetValue(Ordinals.first));
            }
            return results;
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
