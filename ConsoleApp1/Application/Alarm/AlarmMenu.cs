using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Organizer.Application;
using Organizer.Infrastructure;

namespace Organizer
{
    public class AlarmMenu
    {
        public AlarmMenu ToBack { get; set; }
        public List<AlarmMenu> NextMenu { get; set; }
        public Func<State, List<AlarmItem>, Answer> GetAnswer { get; set; }
        public AlarmMenu Root { get; set; }
        public ExpectingRequestFormat RequestFormat { get; set; }
        public Action<State, UiRequest> Action_ { get; set; }
    }
}
