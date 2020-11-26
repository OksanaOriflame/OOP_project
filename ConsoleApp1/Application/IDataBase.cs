using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IDataBase
    {
        public Byte[] GetData(long userId, int dataTypeId, int dataSubTypeId, DateTime time);
        public void SaveData(long userId, int dataTypeId, int dataSubTypeId, DateTime time, Byte[] data);
    }
}
