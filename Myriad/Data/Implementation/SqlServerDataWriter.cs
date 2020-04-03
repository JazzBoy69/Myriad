using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Myriad.Library;

namespace Myriad.Data.Implementation
{
    //todo write change log
    public class SqlServerDataWriter<DataType> : DataWriter<DataType> where DataType : DataObject
    {
        private DataOperation operation;
        protected SqlConnection connection;
        protected SqlCommand command;
        protected SqlTransaction transaction;

        public SqlServerDataWriter(DataOperation operation)
        {
            this.operation = operation;
            connection = SqlServerInfo.Connection();
            connection.Open();
            command = new SqlCommand(SqlServerInfo.Selectors[operation], connection);
        }

        public void AddParameter<T>(int index, T value)
        {
            command.Parameters.AddWithValue(SqlServerInfo.parameterNames[(operation, index)],
                value);
        }

        public void BeginTransaction()
        {
            transaction = connection.BeginTransaction();
            command.Transaction = transaction;
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void SetParameter<T>(int index, T value)
        {
            throw new NotImplementedException();
        }

        public void WriteData(DataType data)
        {
            if (command.Parameters.Count == Number.nothing)
            {
                for (int i = Ordinals.first; i < data.ParameterCount; i++)
                {
                    data.AddParameterTo(this, i);
                }
            }
            command.ExecuteNonQuery();
        }

        public void DeleteData<T>(T value)
        {
            AddParameter(Ordinals.first, value);
            command.ExecuteNonQuery();
        }
    }
}
