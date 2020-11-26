using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IOrganizerItem
    {
        public int GetId();
        public string GetName();
        public RequestHandler GetMessage(RequestHandler requestHandler);
        public void Check(RequestHandler handler);
    }
}
