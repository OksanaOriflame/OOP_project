using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IOrganizerItem
    {
        public int GetId();
        public string GetName();
        public Answer GetAnswer(UiRequest request, State userState);
        public CheckAnswer Check(State userState);
    }
}
