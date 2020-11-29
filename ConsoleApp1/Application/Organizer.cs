using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Asn1.Pkcs;

namespace Organizer
{
    public class Request
    {
        
        public State State { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public Request(State state, DateTime date, string text)
        {
            State = state;
            Date = date;
            Text = text;
        }
    }

    public class State
    {
        public int UserId { get; set; }
        public GlobalStates GlobalState { get; set; }
        public int SubStateId { get; set; }

        public State(int userId, GlobalStates globalState, int subStateId)
        {
            UserId = userId;
            GlobalState = globalState;
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
                new Dictionary<int, IOrganizerItem>(
                    items.Select(item => new KeyValuePair<int, IOrganizerItem>(item.GetId(), item)));
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

        private void ReactOnMessage(Request request)
        {
            var userState = GetUserState(request.State.UserId);
            request.State = userState;
            var answer = GetAnswer(request, userState);
            ui.SendAnswer(answer);
        }

        private Request GetAnswer(Request request, State userState)
        {
            var answer = OrganizerAnswer(userState);
            if (userState.GlobalState == GlobalStates.Organizer)
            {
                if (request.Text.Length > 1)
                {
                    var parseable = int.TryParse(request.Text.Substring(1), out var result);
                    if (parseable && items.ContainsKey(result))
                    {
                        userState.GlobalState = (GlobalStates)result;
                        SaveUserState(userState);
                        answer = items[result].GetMessage(request);
                    }
                }
            }
            else
            {
                answer = items[(int)userState.GlobalState].GetMessage(request);
                if (answer.State.GlobalState == GlobalStates.Organizer)
                    answer = OrganizerAnswer(answer.State);
                SaveUserState(answer.State);
            }
            return answer;
        }

        private Request OrganizerAnswer(State userState)
        {
            var answer = new StringBuilder();
            answer.Append("Выберите номер пункта меню:\n");
            foreach (var item in items)
            {
                answer.Append("/" + item.Key + " - " + item.Value.GetName() + "\n");
            }

            return new Request(userState, default, answer.ToString());
        }

        private State GetUserState(int userId)
        {
            if (!userStates.ContainsKey(userId))
            {
                var isWritten = dataBase.TryGetData(userId, 0, 0, default, out var userData);
                if (!isWritten)
                {
                    userStates[userId] = new State(userId, GlobalStates.Organizer, 0);
                    dataBase.SaveData(userId, 0, 0, default, userStates[userId].ToBytes());
                }
                else
                {
                    userStates[userId] = userData.ToState();
                }
            }
            return userStates[userId];
        }

        private void SaveUserState(State userState)
        {
            userStates[userState.UserId] = userState;
            dataBase.SaveData(userState.UserId, 0, 0, default, userState.ToBytes());
        }
    }
}
