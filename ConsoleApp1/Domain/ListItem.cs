using System;

namespace Organizer
{
    public class ListItem
    {
        public string Text { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DeadLineTime { get; set; }
        public int Priority { get; set; }
    }
}