using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IUi
    {
        public RequestHandler GetNextRequest();
        public void SendAnswer(RequestHandler answer);
        public event Action<RequestHandler> OnMessageRecieved;
    }
}
