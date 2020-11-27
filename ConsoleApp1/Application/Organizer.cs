using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Organizer
{
    public class RequestHandler
    {
        
        public State State { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public RequestHandler(State state, DateTime date, string text)
        {
            State = state;
            Date = date;
            Text = text;
        }
    }

    public class State
    {
        public int UserId { get; set; }
        public int StateId { get; set; }
        public int SubStateId { get; set; }

        public State(int userId, int stateId, int subStateId)
        {
            UserId = userId;
            StateId = stateId;
            SubStateId = subStateId;
        }
    }

    public class Organizer
    {
        private IDataBase dataBase;
        private Dictionary<int, IOrganizerItem> items;
        private IUi ui;
        private Dictionary<int, State> userStates;

        public Organizer(IOrganizerItem[] items, IDataBase dataBase, IUi ui)
        {
            this.items =
                new Dictionary<int, IOrganizerItem>(items.Select(item =>
                    new KeyValuePair<int, IOrganizerItem>(item.GetId(), item)));
            this.dataBase = dataBase;
            this.ui = ui;
            ui.OnMessageRecieved += ReactOnMessage;
            userStates = new Dictionary<int, State>();
        }

        public Dictionary<string, int> GetGlobalOptions()
        {
            return new Dictionary<string, int>(items.Select(item =>
                new KeyValuePair<string, int>(item.Value.GetName(), item.Key)));
        }

        private void ReactOnMessage(RequestHandler request)
        {
            var userState = GetUserState(request.State.UserId);
            ui.SendAnswer(new RequestHandler(request.State, default, "jojo"));
        }

        private State GetUserState(int userId)
        {
            if (!userStates.ContainsKey(userId))
            {
                var userData = dataBase.GetData(userId, 0, 0, default(DateTime));
                if (userData == null)
                {
                    userStates[userId] = new State(userId, 0, 0);
                    dataBase.SaveData(userId, 0, 0, default, userStates[userId].ToBytes());
                }
                else
                {
                    userStates[userId] = userData.ToState();
                }
            }
            return userStates[userId];
        }
    }
}
