using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Organizer
{
    public enum ConditionOfAlarm
    {
        InAlarmMenu,
        InAddAlarmEnterName,
        InAddAlarmEnterDateAndTime,
        InAllAlarmList,
        InRemoveAlarmList
    } 

    public class Alarm : IOrganizerItem
    {
        public int GetId() => 2;
        public string GetName() => "Будильнк";
        private IDataBase dataBase;
        private Dictionary<int, ConditionOfAlarm> conditionMenu = new Dictionary<int, ConditionOfAlarm>();
        private Dictionary<int, AlarmItem> alarmItemForAdd = new Dictionary<int, AlarmItem>();
        private Dictionary<int, int> alarmItemForRemove = new Dictionary<int, int>();
        

        public Alarm(IDataBase dataBase)
        {
            this.dataBase = dataBase;
        }

        public Answer GetAnswer(UiRequest request, State userState)
        {
            if (!conditionMenu.ContainsKey(userState.UserId)) 
            {
                conditionMenu[userState.UserId] = ConditionOfAlarm.InAlarmMenu;
                return AlarmMenu(userState);
            }

            switch (conditionMenu[userState.UserId])
            {
                case ConditionOfAlarm.InAlarmMenu:
                    {
                        if(request.IsBackward)
                        {
                            conditionMenu.Remove(userState.UserId);
                            return Answer.BackWardAnswer(userState.UserId);
                        }

                        if(request.Number == 1)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAddAlarmEnterName;
                            return AddAlarmEnterName(userState);
                        }

                        if(request.Number == 2)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAllAlarmList;
                            return AllAlarmList(userState);
                        }

                        conditionMenu.Remove(userState.UserId);
                        return Answer.BackWardAnswer(userState.UserId);
                    }

                case ConditionOfAlarm.InAddAlarmEnterName:
                    {
                        if(request.IsBackward)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAlarmMenu;
                            return AlarmMenu(userState);
                        }

                        var nameAlarmItem = request.Text;
                        alarmItemForAdd[userState.UserId] = new AlarmItem(nameAlarmItem);

                        conditionMenu[userState.UserId] = ConditionOfAlarm.InAddAlarmEnterDateAndTime;
                        return AddAlarmEnterDateAndTime(userState);
                    }

                case ConditionOfAlarm.InAddAlarmEnterDateAndTime:
                    {
                        if (request.IsBackward)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAlarmMenu;
                            alarmItemForAdd.Remove(userState.UserId);
                            return AlarmMenu(userState);
                        }

                        var dateAndTime = request.DateTime;
                        alarmItemForAdd[userState.UserId].ChangeDateAndTime(dateAndTime);
                        AddAndSaveAlarmItem(userState, alarmItemForAdd[userState.UserId]);
                        alarmItemForAdd.Remove(userState.UserId);

                        conditionMenu[userState.UserId] = ConditionOfAlarm.InAlarmMenu;
                        return AlarmMenu(userState);
                    }

                case ConditionOfAlarm.InAllAlarmList:
                    {
                        if (request.IsBackward)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAlarmMenu;
                            return AlarmMenu(userState);
                        }

                        var alarmItems = GetAlarmItems(userState);
                        
                        if(request.Number <= alarmItems.Count)
                        {
                            alarmItemForRemove[userState.UserId] = request.Number;
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InRemoveAlarmList;
                            return RemoveAlarmItem(userState);
                        }

                        conditionMenu.Remove(userState.UserId);
                        return Answer.BackWardAnswer(userState.UserId);
                    }

                case ConditionOfAlarm.InRemoveAlarmList:
                    {
                        if(request.IsBackward || request.Number == 2)
                        {
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAllAlarmList;
                            return AllAlarmList(userState);
                        }

                        if(request.Number == 1)
                        {
                            RemoveAlarmItem(userState, alarmItemForRemove[userState.UserId]);
                            alarmItemForRemove.Remove(userState.UserId);
                            conditionMenu[userState.UserId] = ConditionOfAlarm.InAllAlarmList;
                            return AllAlarmList(userState);
                        }

                        conditionMenu.Remove(userState.UserId);
                        return Answer.BackWardAnswer(userState.UserId);
                    }

                default:
                    {
                        conditionMenu.Remove(userState.UserId);
                        return Answer.BackWardAnswer(userState.UserId);
                    }
            }
        }

        public CheckAnswer Check(State userState)
        {
            var alarmItems = GetAlarmItems(userState);
            int i = 1;

            foreach(var alarmItem in alarmItems)
            {
                if(alarmItem.DateAndTime == DateTime.Now)
                {
                    RemoveAlarmItem(userState, i);
                    return new CheckAnswer()
                    {
                        HeadLine = "Будильник звонит",
                        Items = new[] { alarmItem.ToString() }
                    };
                }
                i++;
            }
            return null;
        }

        private Answer AlarmMenu(State userState)
        {
            return Answer
                .MenuAnswer(userState.UserId, 
                GetName(), 
                new[] { "Добавить будильник", "Все будильники" });
        }

        private Answer AddAlarmEnterName(State userState)
        {
            return Answer.AskForText(userState.UserId, "Введите название будильника:");
        }

        private Answer AllAlarmList(State userState)
        {
            var alarmItems = GetAlarmItems(userState);

            if (alarmItems.Count() == 0)
                return Answer.EmptyListAnswer(userState.UserId, "Список будильников:");

            return Answer.ListAnswer(userState.UserId, "Список будильников:", alarmItems.Select(alarmItem => alarmItem.ToString()).ToArray());
        }

        private Answer AddAlarmEnterDateAndTime(State userState)
        {
            return Answer.AskForDateAndTime(userState.UserId);
        }

        private Answer RemoveAlarmItem(State userState)
        {
            return Answer.ListAnswer(userState.UserId, "Удалить выбранный будильник?", new string[] { "Да", "Нет" });
        }

        private List<AlarmItem> GetAlarmItems(State userState)
        {

            var alarmItems = new List<AlarmItem>();
            if (!dataBase.TryGetData(userState.UserId, GetId(), 0, default, out byte[] countOfAlarmItems))
                return alarmItems;

            for(int i = 1; i < countOfAlarmItems.ToInt() + 1 ; i++)
            {
                dataBase.TryGetData(userState.UserId, GetId(), i, default, out byte[] alarmItem);

                alarmItems.Add(alarmItem.ToAlarmItem());
            }

            return alarmItems;
        }

        private void AddAndSaveAlarmItem(State userState, AlarmItem alarmItem = null, List<AlarmItem> alarmItems = null)
        {
            if (alarmItems is null)
                alarmItems = GetAlarmItems(userState);
            if (alarmItem != null)
                alarmItems.Add(alarmItem);
            alarmItems = alarmItems.OrderBy(alI => alI.DateAndTime).ToList();

            int i = 1;
            foreach(var alI in alarmItems)
            {
                dataBase.SaveData(userState.UserId, GetId(), i, default, alI.ToBytes());
                i++;
            }

            dataBase.SaveData(userState.UserId, GetId(), 0, default, (i - 1).ToBytes());
        }

        private void RemoveAlarmItem(State userState, int subTypeId)
        {
            var alarmItems = GetAlarmItems(userState);
            dataBase.RemoveData(userState.UserId, GetId(), subTypeId, default);
            dataBase.RemoveData(userState.UserId, GetId(), alarmItems.Count, default);
            alarmItems.RemoveAt(subTypeId - 1);

            AddAndSaveAlarmItem(userState, null, alarmItems);
        }
    }
}
