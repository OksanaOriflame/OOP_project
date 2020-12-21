using System;

namespace Organizer.Application
{
    public class UiRequest
    {
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public bool IsBackward { get; set; }
        public bool IsShowThisItem { get; set; }
    }
}