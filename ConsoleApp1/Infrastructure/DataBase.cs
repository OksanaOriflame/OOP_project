using System;
using System.Collections.Generic;

namespace Organizer
{
    public class DataBase : IDataBase
    {
        private Dictionary<long, Dictionary<int, Dictionary<DateTime, byte[]>>> data;
        public DataBase()
        {
            data = new Dictionary<long, Dictionary<int, Dictionary<DateTime, byte[]>>>();
        }
        public byte[] GetData(long userId, int dataTypeId, int subDataTypeId, DateTime time)
        {
            return data[userId][dataTypeId][time];
        }

        public void SaveData(long userId, int dataTypeId, int subDataTypeId, DateTime time, byte[] data)
        {
            this.data[userId][dataTypeId][time] = data;
        }
    }
}
