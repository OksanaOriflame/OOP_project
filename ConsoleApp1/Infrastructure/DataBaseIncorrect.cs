using System;
using System.Collections.Generic;

namespace Organizer
{
    public class DataBaseIncorrect : IDataBaseIncorrect
    {
        private Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<DateTime, byte[]>>>> data;
        public DataBaseIncorrect()
        {
            data = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<DateTime, byte[]>>>>();
        }
        public byte[] GetData(int userId, int dataTypeId, int subDataTypeId, DateTime time)
        {
            if (!data.ContainsKey(userId))
            {
                return null;
            }
            return data[userId][dataTypeId][subDataTypeId][time];
        }

        public void SaveData(int userId, int dataTypeId, int subDataTypeId, DateTime time, byte[] data)
        {
            var dict = this.data;
            if (!dict.ContainsKey(userId))
                dict[userId] = new Dictionary<int, Dictionary<int, Dictionary<DateTime, byte[]>>>();
            if (!dict[userId].ContainsKey(dataTypeId))
                dict[userId][dataTypeId] = new Dictionary<int, Dictionary<DateTime, byte[]>>();
            if (!dict[userId][dataTypeId].ContainsKey(subDataTypeId))
                dict[userId][dataTypeId][subDataTypeId] = new Dictionary<DateTime, byte[]>();
            this.data[userId][dataTypeId][subDataTypeId][time] = data;
        }
    }
}
