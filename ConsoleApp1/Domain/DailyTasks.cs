using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Organizer
{
    enum DailyTasksViewingState
    {
        Listing,
        Concrete
    }

    enum DailyTasksAddingState
    {
        AddingMenu,
        AddingMenuWaitingForAnswer,
        AddingDate,
        AddingTask
    }
    
    public class DailyTasks : AbstractThemeList
    {
        private Dictionary<int, DailyTasksViewingState> currentViewingStates;
        private Dictionary<int, DailyTasksAddingState> currentAddingState;
        private Dictionary<int, Func<UiRequest, State, Answer>> AddingMenuOptions;
        public override int GetId() => 2;
        public override string GetName() => "Ежедневные дела";
        protected override int GetIdWithOffset() => 1000 + GetId();
        protected override byte[] ConvertListItemToBytes(ListTaskItem taskItem)
        {
            return taskItem.Text.ToBytes();
        }

        protected override ListTaskItem ParseListItemFromBytes(byte[] bytes)
        {
            return new ListTaskItem() {Text = bytes.To_String()};
        }

        public DailyTasks(IDataBase dataBase) : base(dataBase)
        {
            currentViewingStates = new Dictionary<int, DailyTasksViewingState>();
            currentAddingState = new Dictionary<int, DailyTasksAddingState>();
            AddingMenuOptions = new Dictionary<int, Func<UiRequest, State, Answer>>()
            {
                [1] = AddingNewDateAnswer,
                [2] = AddingNewTaskAnswer
            };
        }

        protected override Answer ExistingTasksViewingAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!currentViewingStates.ContainsKey(userId))
            {
                currentViewingStates[userId] = DailyTasksViewingState.Listing;
            }
            if (currentViewingStates[userId] == DailyTasksViewingState.Listing)
            {
                currentViewingStates[userId] = DailyTasksViewingState.Concrete;
                return AnswerAllDates(userId);
            }
            else
            {
                if (request.IsBackward)
                {
                    currentViewingStates[userId] = DailyTasksViewingState.Listing;
                    return Answer.BackWardAnswer(userId);
                }

                return AnswerAllDates(userId);
            }
        }

        public Answer AnswerAllDates(int userId)
        {
            var datesCount = GetTasksCount(userId, default);
            if (datesCount == 0)
            {
                return Answer.EmptyListAnswer(userId, "Нет списков дел");
            }
            var dates = GetAllDates(userId, datesCount);
            return Answer.ListAnswer(userId, "Выберите Дату", dates.Select(date => date.ToString()).ToArray());
        }

        private DateTime[] GetAllDates(int userId, int datesCount)
        {
            var isInDataBase = dataBase.TryGetData(userId, GetIdWithOffset(), 1, default, out var timesBytes);
            if (!isInDataBase)
            {
                timesBytes = 0.ToBytes();
                dataBase.SaveData(userId, GetIdWithOffset(), 1, default, timesBytes);
                return new DateTime[0];
            }
            var dates = new List<DateTime>();
            for (var i = 0; i < datesCount; i += 8)
            {
                var oneDate = new List<byte>();
                for (var j = i; j < i + 8; j++)
                {
                    oneDate.Add(timesBytes[j]);
                }
                
                dates.Add(oneDate.ToArray().ToDateTime());
            }

            return dates.ToArray();
        }

        protected override Answer TaskAddingAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!currentAddingState.ContainsKey(userId))
            {
                currentAddingState[userId] = DailyTasksAddingState.AddingMenu;
            }

            var answer = AddingMenuAnswer(userId);
            switch (currentAddingState[userId])
            {
                case DailyTasksAddingState.AddingMenu:
                {
                    currentAddingState[userId] = DailyTasksAddingState.AddingMenuWaitingForAnswer;
                    return answer;
                }
                case DailyTasksAddingState.AddingMenuWaitingForAnswer:
                {
                    if (request.IsBackward)
                    {
                        currentAddingState[userId] = DailyTasksAddingState.AddingMenu;
                        return Answer.BackWardAnswer(userId);
                    }

                    request.IsShowThisItem = true;
                    answer = AddingMenuOptions[request.Number](request, userState);
                    break;
                }
                case DailyTasksAddingState.AddingDate:
                {
                    answer = AddingNewDateAnswer(request, userState);
                    break;
                }
            }

            if (answer.IsBackward)
            {
                currentAddingState[userId] = DailyTasksAddingState.AddingMenuWaitingForAnswer;
                answer = AddingMenuAnswer(userId);
            }
            return answer;
        }

        private Answer AddingMenuAnswer(int userId)
        {
            return Answer.MenuAnswer(userId, "Новая задача", new string[] {"Добавить новую дату", "Добавить дело на существующую дату"});
        }

        private Answer AddingNewDateAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (request.IsShowThisItem)
            {
                currentAddingState[userId] = DailyTasksAddingState.AddingDate;
                return Answer.AskForDate(userId);
            }

            if (request.IsBackward)
            {
                return Answer.BackWardAnswer(userId);
            }

            var datesCount = GetTasksCount(userId, default);
            var dates = GetAllDates(userId, datesCount);
            if (dates.Contains(request.DateTime))
            {
                return Answer.DateIsAlreadyExists(userId);
            }
            
            
            return default;
        }

        private void SaveNewDate(int userId, int datesCount, DateTime dateTime)
        {
            var dates = GetAllDates(userId, datesCount).ToList();
            datesCount++;
            dates.Add(dateTime);
            dates = dates.OrderBy(date => date).ToList();
            var datesBytes = new List<byte>();
            foreach (var date in dates)
            {
                datesBytes = datesBytes.Concat(date.ToBytes()).ToList();
            }
            dataBase.SaveData(userId, GetIdWithOffset(), 0, default, datesCount.ToBytes());
            dataBase.SaveData(userId, GetIdWithOffset(), 1, default, datesBytes.ToArray());
        }

        private Answer AddingNewTaskAnswer(UiRequest request, State userState)
        {
            return default;
        }
    }
}