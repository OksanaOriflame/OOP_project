using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Organizer.Infrastructure;

namespace Organizer.Application
{
    public class AlarmOrganizerItem : IOrganizerItem
    {
        public int GetId() => 2;
        public string GetName() => "Будильник";
        private IDataBase dataBase;
        private AlarmMenu alarmMenu;
        private Dictionary<int, AlarmMenu> alarmMenuForUser = new Dictionary<int, AlarmMenu>();
        private Dictionary<int, Alarm> alarmForUser = new Dictionary<int, Alarm>();
        private Dictionary<int, AlarmItem> alarmItemForAdd = new Dictionary<int, AlarmItem>();
        private Dictionary<int, AlarmItem> alarmItemForEdit = new Dictionary<int, AlarmItem>();

        public AlarmOrganizerItem()
        {
            alarmMenu = CreateMenu();
        }

        public void ConnectToDataBase(IDataBase dataBase)
        {
            this.dataBase = dataBase;
        }

        public Answer GetAnswer(UiRequest request, State userState)
        {
            if (!alarmMenuForUser.ContainsKey(userState.UserId))
            {
                alarmForUser[userState.UserId] = GetAlarm(userState);
                alarmMenuForUser[userState.UserId] = alarmMenu.Root;
                return alarmMenuForUser[userState.UserId].GetAnswer(userState, alarmForUser[userState.UserId].GetAllAlarms());
            }

            if (request.IsBackward)
            {
                alarmMenuForUser[userState.UserId] = alarmMenuForUser[userState.UserId].ToBack;
                if (alarmMenuForUser[userState.UserId] == null)
                {
                    RewriteAlarmItems(userState);
                    alarmMenuForUser.Remove(userState.UserId);
                    alarmForUser.Remove(userState.UserId);
                    return Answer.BackWardAnswer(userState.UserId);
                }

                return alarmMenuForUser[userState.UserId].GetAnswer(userState, alarmForUser[userState.UserId].GetAllAlarms());
            }

            alarmMenuForUser[userState.UserId].Action_(userState, request);
            return alarmMenuForUser[userState.UserId].GetAnswer(userState, alarmForUser[userState.UserId].GetAllAlarms());


            throw new NotImplementedException();
        }

        public CheckAnswer Check(State userState)
        {
            var alarmItems = alarmForUser[userState.UserId].GetAllAlarms();

            foreach (var alarmItem in alarmItems)
            {
                if (alarmItem.Time.TimeOfDay == DateTime.Now.TimeOfDay)
                {
                    alarmForUser[userState.UserId].RemoveAlarm(alarmItem);
                    return new CheckAnswer()
                    {
                        HeadLine = "Будильник звонит",
                        Items = new[] { alarmItem.ToString() }
                    };
                }
            }
            return null;
        }

        private AlarmMenu CreateMenu()
        {
            var rootMenu = new AlarmMenu()
            {
                ToBack = null,
                GetAnswer = (userState, alarmItems) => Answer
                        .MenuAnswer(userState.UserId,
                        GetName(),
                        new[] { "Все будильники", "Добавить будильник" }),
                NextMenu = new List<AlarmMenu>(),
                RequestFormat = ExpectingRequestFormat.Number,
                Action_ = SelectNextMenu
            };

            rootMenu.Root = rootMenu;

            var enterNameMenu = new AlarmMenu()
            {
                ToBack = rootMenu,
                Root = rootMenu,
                GetAnswer = (userState, alarmItems) => Answer.AskForText(userState.UserId, "Добавление будильника\nВведите название будильника:"),
                NextMenu = new List<AlarmMenu>(),
                RequestFormat = ExpectingRequestFormat.Text,
                Action_ = CreateAlarmItemWithOutTime
            };

            var allAlarmList = new AlarmMenu()
            {
                ToBack = rootMenu,
                Root = rootMenu,
                GetAnswer = (userState, alarmItems) => Answer.ListAnswer(userState.UserId, "Список будильников:", alarmItems.Select(alarmItem => alarmItem.ToString()).ToArray()),
                NextMenu = new List<AlarmMenu>(),
                RequestFormat = ExpectingRequestFormat.Number,
                Action_ = SelectAlarmItem
            };

            rootMenu.NextMenu.Add(allAlarmList);
            rootMenu.NextMenu.Add(enterNameMenu);

            var enterTimeMenu = new AlarmMenu()
            {
                ToBack = rootMenu,
                Root = rootMenu,
                GetAnswer = (userState, alarmItems) => Answer.AskForTime(userState.UserId),
                NextMenu = new List<AlarmMenu>() { rootMenu },
                RequestFormat = ExpectingRequestFormat.OnlyTimeDateTime,
                Action_ = AddAlarmItemWithTime
            };

            enterNameMenu.NextMenu.Add(enterTimeMenu);

            var pitchAlarmMenu = new AlarmMenu()
            {
                ToBack = allAlarmList,
                Root = rootMenu,
                GetAnswer = (userState, alarmItems) => Answer
                .ListAnswer(userState.UserId, "Что вы хотите сделать, с выбранным будильником:", new string[] { "Удалить", "Изменить" }),
                NextMenu = new List<AlarmMenu>(),
                RequestFormat = ExpectingRequestFormat.Number,
                Action_ = SelectNextMenu
            };

            allAlarmList.NextMenu.Add(pitchAlarmMenu);

            var removeAlarmItemMenu = new AlarmMenu()
            {
                ToBack = allAlarmList,
                Root = rootMenu,
                GetAnswer = (userState, alarmItems) => Answer.ListAnswer(userState.UserId, string.Format("Удалить будильник {0}?", alarmItemForEdit[userState.UserId].ToString()), new string[] { "Да", "Нет" }),
                NextMenu = new List<AlarmMenu>() { allAlarmList },
                RequestFormat = ExpectingRequestFormat.Number,
                Action_ = RemoveAlarmItem
            };

            var editAlarmItemMenu = new AlarmMenu()
            {
                ToBack = allAlarmList,
                Root = rootMenu,
                GetAnswer = (userState, alarmItem) => Answer.ListAnswer(userState.UserId, string.Format("Изменить время или название\nбудильника {0}?", alarmItemForEdit[userState.UserId].ToString()), new string[] { "Изменить время", "Изменить название" }),
                NextMenu = new List<AlarmMenu>(),
                RequestFormat = ExpectingRequestFormat.Number,
                Action_ = SelectNextMenu
            };

            pitchAlarmMenu.NextMenu.Add(removeAlarmItemMenu);
            pitchAlarmMenu.NextMenu.Add(editAlarmItemMenu);

            var editTime = new AlarmMenu()
            {
                ToBack = allAlarmList,
                Root = rootMenu,
                GetAnswer = (userState, alarmItem) => new Answer
                {
                    Format = ExpectingRequestFormat.OnlyTimeDateTime,
                    UserId = userState.UserId,
                    Headline = string.Format("Введите новое время для\nбудильника {0}:", alarmItemForEdit[userState.UserId].ToString()),
                    IsBackward = false,
                    Items = new string[0]
                },
                NextMenu = new List<AlarmMenu>() { allAlarmList },
                RequestFormat = ExpectingRequestFormat.OnlyTimeDateTime,
                Action_ = EditTimeAlarmItem
            };

            var editName = new AlarmMenu()
            {
                ToBack = allAlarmList,
                Root = rootMenu,
                GetAnswer = (userState, alarmItem) => Answer.AskForText(userState.UserId, string.Format("Введите новое название для\nбудильника {0}:", alarmItemForEdit[userState.UserId].ToString())),
                NextMenu = new List<AlarmMenu>() { allAlarmList },
                RequestFormat = ExpectingRequestFormat.Text,
                Action_ = EditNameAlarmItem
            };

            editAlarmItemMenu.NextMenu.Add(editTime);
            editAlarmItemMenu.NextMenu.Add(editName);

            return rootMenu;
        }

        private Alarm GetAlarm(State userState)
        {
            if (!dataBase.TryGetData(userState.UserId, GetId(), 0, default, out byte[] countOfAlarmItems))
                return new Alarm();

            var alarmItems = new List<AlarmItem>();

            for (int i = 1; i < countOfAlarmItems.ToInt() + 1; i++)
            {
                dataBase.TryGetData(userState.UserId, GetId(), i, default, out byte[] alarmItem);
                alarmItems.Add(alarmItem.ToAlarmItem());
            }

            return new Alarm(alarmItems);
        }

        private void RewriteAlarmItems(State userState)
        {
            if (dataBase.TryGetData(userState.UserId, GetId(), 0, default, out byte[] countOfAlarmItems))
                for (int i = 1; i < countOfAlarmItems.ToInt() + 1; i++)
                    dataBase.RemoveData(userState.UserId, GetId(), i, default);

            var alarmItems = alarmForUser[userState.UserId].GetAllAlarms();
            var count = alarmItems.Count();
            dataBase.SaveData(userState.UserId, GetId(), 0, default, count.ToBytes());

            for (int i = 1; i < count + 1; i++)
                dataBase.SaveData(userState.UserId, GetId(), i, default, alarmItems[i - 1].ToBytes());
        }

        private void CreateAlarmItemWithOutTime(State userState, UiRequest request)
        {
            alarmItemForAdd[userState.UserId] = new AlarmItem(request.Text);
            SelectNextMenu(userState);
        }

        private void AddAlarmItemWithTime(State userState, UiRequest request)
        {
            alarmItemForAdd[userState.UserId].ChangeTime(request.DateTime);
            alarmForUser[userState.UserId].AddAlarm(alarmItemForAdd[userState.UserId]);
            alarmItemForAdd.Remove(userState.UserId);
            SelectNextMenu(userState);
        }

        private void SelectNextMenu(State userState, UiRequest request = null)
        {
            int number = 1;
            if (request != null)
                number = request.Number;
            alarmMenuForUser[userState.UserId] = alarmMenuForUser[userState.UserId].NextMenu[number - 1];
        }

        private void SelectAlarmItem(State userState, UiRequest request)
        {
            alarmItemForEdit[userState.UserId] = alarmForUser[userState.UserId].GetAllAlarms()[request.Number - 1];
            SelectNextMenu(userState);
        }

        private void RemoveAlarmItem(State userState, UiRequest request)
        {
            if (request.Number == 1)
                alarmForUser[userState.UserId].GetAllAlarms().Remove(alarmItemForEdit[userState.UserId]);
            SelectNextMenu(userState);
        }

        private void EditTimeAlarmItem(State userState, UiRequest request)
        {
            alarmItemForEdit[userState.UserId].ChangeTime(request.DateTime);
            SelectNextMenu(userState);
        }

        private void EditNameAlarmItem(State userState, UiRequest request)
        {
            alarmItemForEdit[userState.UserId].ChangeName(request.Text);
            SelectNextMenu(userState);
        }
    }
}