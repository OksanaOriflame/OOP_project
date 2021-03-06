﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Asn1.Pkcs;
using Organizer.Infrastructure;

namespace Organizer.Application
{
    public class State
    {
        public int UserId { get; }
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
        private Dictionary<GlobalStates, IOrganizerItem> items;
        private Dictionary<int, State> userStates;
        private Thread checker;
        public event Action<int, string, string[], ExpectingRequestFormat> OnReplyRequest;

        public Organizer(IOrganizerItem[] items, IDataBase dataBase)
        {
            this.items =
                new Dictionary<GlobalStates, IOrganizerItem>(
                    items.Select(item => new KeyValuePair<GlobalStates, IOrganizerItem>((GlobalStates)item.GetId(), item)));
            this.dataBase = dataBase;
            foreach (var item in items)
            {
                item.ConnectToDataBase(dataBase);
            }
            userStates = new Dictionary<int, State>();
            checker = new Thread(Check);
        }

        public void Start()
        {
            while (true)
            {
                Check();
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        private void Check()
        {
            /*var checkAnswers = new List<CheckAnswer>();
            foreach (var item in items)
            {
                var answer = item.Value.Check(default);
            }*/
        }

        public void ProcessMessage(int userId, DateTime date, string text, int number, bool isBackward, bool isShowThisItem)
        {
            var request = new UiRequest()
            {
                DateTime = date,
                IsBackward = isBackward,
                IsShowThisItem = isShowThisItem,
                Number = number,
                Text = text,
                UserId = userId
            };
            var userState = GetUserState(request.UserId);
            var answer = GetAnswer(request, userState);
            OnReplyRequest?.Invoke(answer.UserId, answer.Headline, answer.Items, answer.Format);
        }

        private Answer GetAnswer(UiRequest request, State userState)
        {
            if (userState.GlobalState == GlobalStates.Organizer)
            {
                if (request.IsShowThisItem || request.IsBackward)
                {
                    return OrganizerAnswer(userState);
                }

                userState.GlobalState = (GlobalStates) request.Number;
                request.IsShowThisItem = true;
            }

            var answer = items[userState.GlobalState].GetAnswer(request, userState);
            if (answer.IsBackward)
            {
                userState.GlobalState = GlobalStates.Organizer;
                answer = OrganizerAnswer(userState);
            }
            SaveUserState(userState);
            return answer;
        }

        private Answer OrganizerAnswer(State userState)
        {
            return new Answer()
            {
                UserId = userState.UserId,
                Format = ExpectingRequestFormat.Number, 
                NumberRange = new Tuple<int, int>(1, items.Count), 
                Headline = "Выберите номер пункта меню:", 
                Items = items
                    .OrderBy(item => item.Key)
                    .Select(item => item.Value.GetName()).ToArray(),
                IsBackward = false
            };
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
