using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public class AlarmItem
    {
        public string Name { get; private set; }
        public DateTime DateAndTime { get; private set; }

        public AlarmItem(string name, DateTime dateAndTime = default)
        {
            Name = name;
            DateAndTime = dateAndTime;
        }

        public override string ToString()
        {
            return string.Format("{0} - Дата: {1}, Время: {2} ", Name, DateAndTime.ToLongDateString(), DateAndTime.ToLongTimeString());
        }

        public void ChangeDateAndTime(DateTime dateAndTime)
        {
            DateAndTime = dateAndTime;
        } 
    }
}