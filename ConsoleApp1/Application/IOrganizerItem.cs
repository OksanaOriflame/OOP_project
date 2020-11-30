using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IOrganizerItem
    {
        public int GetId();
        public string GetName();
        public Answer GetMessage(UiRequest request, State userState);
        public void Check(Request handler);
    }
}
