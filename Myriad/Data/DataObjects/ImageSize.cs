using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using System.Data.Common;

namespace Myriad.Data
{
    internal class ImageSize : DataObject
    {
        double height;
        double width;
        public void Read(DbDataReader reader)
        {
            height = reader.GetDouble(Ordinals.first);
            width = reader.GetDouble(Ordinals.second);
        }

        public double Height
        {
            get
            {
                return height;
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
        }
    }
}
