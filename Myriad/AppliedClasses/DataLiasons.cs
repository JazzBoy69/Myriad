using System.Data.SqlClient;

namespace Myriad.AppliedClasses
{
    class DataCommandLiason
    {
        internal SqlCommand Command { get; set; }
        internal DataCommandLiason(string commandText)
        {
            Command = new SqlCommand(commandText);
        }
    }

    internal class DataConnectionLiason
    {
        const string connectionString = "Server=.\\SQLExpress;Initial Catalog=Myriad;Trusted_Connection=Yes;";
        internal SqlConnection Connection { get; set; }
        internal DataConnectionLiason()
        {
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }
    }


    internal class DataReaderLiason
    {
        internal SqlDataReader Reader { get; set; }

        internal DataReaderLiason(DataCommandLiason commandLiason)
        {
            Reader = commandLiason.Command.ExecuteReader();
        }
    }
}
