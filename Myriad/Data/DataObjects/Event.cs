using Feliciana.Data;
using Feliciana.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class Event : DataObject
    {
        int chronoID;
        int chronoIndex;
        int xoffset;
        int yoffset;
        string pictureFile;
        public int ID => chronoID;
        public int Index => chronoIndex;
        public string PictureFile => "pictures/"+pictureFile;
        public string Picture => pictureFile;
public string Offset
        {
            get
            {
                if ((xoffset == 50) && (yoffset == 50)) return " ";
                return " style='object-position: "+xoffset+"% "+yoffset+"% ' ";
            }
        }
        public int ParameterCount => throw new NotImplementedException();

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            chronoID = await reader.GetFieldValueAsync<int>(Ordinals.first);
            chronoIndex = await reader.GetFieldValueAsync<int>(Ordinals.second);
            pictureFile = await reader.GetFieldValueAsync<string>(Ordinals.third);
            xoffset = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            yoffset = await reader.GetFieldValueAsync<int>(Ordinals.fifth);
        }

        public void ReadSync(DbDataReader reader)
        {
            chronoID = reader.GetFieldValue<int>(Ordinals.first);
            chronoIndex = reader.GetFieldValue<int>(Ordinals.second);
            pictureFile = reader.GetFieldValue<string>(Ordinals.third);
            xoffset = reader.GetFieldValue<int>(Ordinals.fourth);
            yoffset = reader.GetFieldValue<int>(Ordinals.fifth);
        }
    }
}
