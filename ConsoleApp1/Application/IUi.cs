using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IUi
    {
        public void SendAnswer(Answer answer);
        public event Action<UiRequest> OnMessageRecieved;
    }
}
