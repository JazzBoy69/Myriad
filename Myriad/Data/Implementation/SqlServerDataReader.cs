using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Feliciana.Library;

namespace Myriad.Data.Implementation
{
    public class SqlServerDataReader<KeyType> : DataReader<KeyType>
    {
        protected SqlDataReader reader;
        protected SqlConnection connection;
        protected SqlCommand command;
        protected DataOperation operation;
        public SqlServerDataReader(DataOperation operation, KeyType key)
        {
            this.operation = operation;
            connection = SqlServerInfo.Connection();
            connection.Open();
            command = new SqlCommand(SqlServerInfo.Commands[operation], connection);
            command.Parameters.AddWithValue(SqlServerInfo.parameterNames[(operation, Ordinals.first)], key);
        }

        public void Open()
        {
            connection = SqlServerInfo.Connection();
            connection.Open();
            command.Connection = connection;
        }

        public List<DataType> GetData<DataType>()
        {
            reader = command.ExecuteReader();
            List<DataType> results = new List<DataType>();
            while (reader.Read())
            {
                results.Add(reader.GetFieldValue<DataType>(Ordinals.first));
            }
            reader.Close();
            return results;
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
            reader.Close();
            return results;
        }

        public DataType GetDatum<DataType>()
        {
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                DataType result = reader.GetFieldValue<DataType>(Ordinals.first);
                reader.Close();
                return result;
            }
            reader.Close();
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
            reader.Close();
            return results;
        }

        public ClassType GetClassDatum<ClassType>() where ClassType : DataObject, new()
        {
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                ClassType result = DataObjectFactory<ClassType>.Read(reader);
                reader.Close();
                return result;
            }
            reader.Close();
            return default;
        }

        public (T1, T2) GetDatum<T1, T2>()
        {
            reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                var result = (reader.GetFieldValue<T1>(Ordinals.first),
                    reader.GetFieldValue<T2>(Ordinals.second));
                reader.Close();
                return result;
            }
            reader.Close();
            return default;
        }

        public void Close()
        {
            reader.Close();
            command.Dispose();
            connection.Close();
        }


        public void SetParameter(KeyType parameter)
        {
            command.Parameters[SqlServerInfo.parameterNames[(operation, Ordinals.first)]].Value = parameter;
        }
    }

    public class SqlServerDataReader<KeyType1, KeyType2> : SqlServerDataReader<KeyType1>, DataReader<KeyType1,KeyType2> 
    {
        public SqlServerDataReader(DataOperation operation, KeyType1 key1, KeyType2 key2) : base(operation, key1)    
        {
            command.Parameters.AddWithValue(SqlServerInfo.parameterNames[(operation, Ordinals.second)], key2);
        }

        public void SetParameter(KeyType1 parameter1, KeyType2 parameter2) 
        {
            base.SetParameter(parameter1);
            command.Parameters[SqlServerInfo.parameterNames[(operation, Ordinals.second)]].Value = parameter2;
        }
    }
}
