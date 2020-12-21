using System;

namespace Organizer.Infrastructure
{
    public class DataBaseParams
    {
        public int UserId { get; }
        public int StateId { get; }
        public int SubStateId { get; }
        public DateTime Date { get; }

        public DataBaseParams(int userId, int stateId, int subStateId, DateTime date)
        {
            UserId = userId;
            StateId = stateId;
            SubStateId = subStateId;
            Date = date;
        }
    }
}