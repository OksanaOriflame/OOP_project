using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public class AlarmItem
    {
        public string Name { get; private set; }
        public DateTime Time { get; private set; }

        public AlarmItem(string name, DateTime time = default)
        {
            Name = name;
            Time = time;
        }

        public override string ToString()
        {
            return string.Format("{0} - Время: {1} ", Name, Time.ToLongTimeString());
        }

        public void ChangeTime(DateTime time)
        {
            Time = time;
        }
        
        public void ChangeName(string name)
        {
            Name = name;
        }
    }
}