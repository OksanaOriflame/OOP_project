using System;
using System.Collections.Generic;
using System.Text;

namespace Organizer
{
    public interface IUi
    {
        public void Start();
        public RequestHandler GetNextRequest();
        public void SendAnswer(RequestHandler answer);
    }
}
