using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IUi
    {
        public void SendAnswer(Request answer);
        public event Action<Request> OnMessageRecieved;
    }
}
