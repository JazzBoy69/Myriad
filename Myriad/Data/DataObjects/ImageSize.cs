using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using System.Data.Common;
using System.Data.SqlClient;

namespace Myriad.Data
{
    public class ImageSize : DataObject
    {
        double height;
        double width;
        string filename;
        public double Height => height;
        public double Width => width;
        public string Filename => filename;

        public int ParameterCount => 3;
        public ImageSize()
        {
        }

        public ImageSize(string filename, double width, double height)
        {
            this.filename = filename;
            this.width = width;
            this.height = height;
        }
        public void Read(DbDataReader reader)
        {
            width = reader.GetDouble(Ordinals.first);
            height = reader.GetDouble(Ordinals.second);
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }
        public void AddParameterTo<DataType>(DataWriter<DataType> writer, int index) where DataType : DataObject
        {
            switch (index)
            {
                case Ordinals.first:
                    {
                        writer.AddParameter(index, filename);
                        break;
                    }
                case Ordinals.second:
                    {
                        writer.AddParameter(index, width);
                        break;
                    }
                case Ordinals.third:
                    {
                        writer.AddParameter(index, height);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
