using System;
using System.Collections.Generic;

namespace Organizer
{
    public class DataBase : IDataBase
    {
        private Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<DateTime, byte[]>>>> data;
        public DataBase()
        {
            data = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<DateTime, byte[]>>>>();
        }
        public byte[] GetData(int userId, int dataTypeId, int subDataTypeId, DateTime time)
        {
            return data[userId][dataTypeId][subDataTypeId][time];
        }

        public void SaveData(int userId, int dataTypeId, int subDataTypeId, DateTime time, byte[] data)
        {
            this.data[userId][dataTypeId][subDataTypeId][time] = data;
        }
    }
}
