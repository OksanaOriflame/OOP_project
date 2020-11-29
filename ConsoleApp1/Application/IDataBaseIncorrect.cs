using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IDataBaseIncorrect
    {
        public Byte[] GetData(int userId, int dataTypeId, int dataSubTypeId, DateTime time);
        public void SaveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, Byte[] data);
    }
    
    public interface IDataBase
    {
        public bool TryGetData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, out byte[] data);
        public void SaveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, Byte[] data);
        public void RemoveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time);
    }
}
