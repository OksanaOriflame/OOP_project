using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IDataBase
    {
        public Byte[] GetData(int userId, int dataTypeId, int dataSubTypeId, DateTime time);
        public void SaveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, Byte[] data);
    }
}
