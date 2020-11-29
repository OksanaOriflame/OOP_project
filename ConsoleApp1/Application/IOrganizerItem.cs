using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IOrganizerItem
    {
        public int GetId();
        public string GetName();
        public Request GetMessage(Request request);
        public void Check(Request handler);
    }
}
