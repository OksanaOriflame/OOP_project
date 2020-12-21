using System;
using System.Collections.Generic;
using System.Text;
using Organizer.Infrastructure;

namespace Organizer.Application.Interfaces
{
    public interface IUi
    {
        public event Action<int, DateTime, string, int, bool, bool> OnMessageRecieved;
    }
}
