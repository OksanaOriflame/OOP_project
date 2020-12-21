using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Organizer
{
    public class Alarm
    {
        private List<AlarmItem> alarmItems;

        public Alarm()
        {
            alarmItems = new List<AlarmItem>();
        }

        public Alarm(List<AlarmItem> alarmItems)
        {
            this.alarmItems = alarmItems;
        }

        public void AddAlarm(AlarmItem alarmItem)
        {
            alarmItems.Add(alarmItem);
            alarmItems.OrderBy(alarmItem => alarmItem.Time);
        }

        public void AddAlarm(string name, int hours, int minutes, int seconds = 00)
        {
            DateTime time = default;
            time.AddHours(hours);
            time.AddMinutes(minutes);
            time.AddSeconds(seconds);

            AddAlarm(new AlarmItem(name, time));
        }

        public void RemoveAlarm(AlarmItem alarmItem)
        {
            alarmItems.Remove(alarmItem);
        }

        public void RemoveAlarm(string name, int hours, int minutes, int seconds = 00)
        {
            DateTime time = default;
            time.AddHours(hours);
            time.AddMinutes(minutes);
            time.AddSeconds(seconds);

            RemoveAlarm(new AlarmItem(name, time));
        }

        public List<AlarmItem> GetAllAlarms()
        {
            return alarmItems;
        }
    }
}
