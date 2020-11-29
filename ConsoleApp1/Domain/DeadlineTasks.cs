using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Ocsp;

namespace Organizer
{
    enum ThemeListState
    {
        Menu = 0,
        TasksViewing = 1,
        Adding = 2
    }

    enum AddingStates
    {
        Deadline,
        Text
    }
    
    public class DeadlineTasks : IThemeList
    {
        public int GetId() => 1;
        private int idWithOffset => GetId() + 100;
        public string GetName() => "Дела с дедлайном";
        private bool isActive;
        private Dictionary<int, ThemeListState> currentStates;
        private Dictionary<int, AddingStates> currentAddingStates;
        private Dictionary<int, ListItem> newItems;
        private IDataBase dataBase;

        public DeadlineTasks(IDataBase dataBase)
        {
            isActive = false;
            currentStates = new Dictionary<int, ThemeListState>();
            currentAddingStates = new Dictionary<int, AddingStates>();
            newItems = new Dictionary<int, ListItem>();
            this.dataBase = dataBase;
        }
        
        public Request GetAnswer(Request request)
        {
            if (!isActive)
            {
                currentStates[request.State.UserId] = ThemeListState.Menu;
                return MenuAnswer(request);
            }

            switch (currentStates[request.State.UserId])
            {
                case ThemeListState.Menu:
                    return MenuAnswer(request);
                case ThemeListState.Adding:
                    if (currentAddingStates[request.State.UserId] == AddingStates.Deadline)
                    {
                        return AnswerDeadlineAdding(request);
                    }

                    return AnswerTaskTextAdding(request);
                case ThemeListState.TasksViewing:
                    return AnswerTasksViewing(request);
            }

            return MenuAnswer(request);
        }

        private Request MenuAnswer(Request request)
        {
            if (request.Text.ToLower() == "/back")
            {
                request.State.SubStateId = 0;
                request.Text = "";
                isActive = false;
                return request;
            }

            if (isActive && request.Text.Length > 1)
            {
                switch (request.Text)
                {
                    case "/1":
                        currentStates[request.State.UserId] = ThemeListState.TasksViewing;
                        return GetTasks(request);
                    case "/2":
                        currentStates[request.State.UserId] = ThemeListState.Adding;
                        currentAddingStates[request.State.UserId] = AddingStates.Deadline;
                        request.Text = "Укажите дедлайн\n/Back";
                        return request;
                }
            }
            var answer = "Выберите номер пункта меню:\n/1 - Посмотреть дела\n/2 - Добавить новое дело\n/Back";
            request.Text = answer;
            isActive = true;
            return request;
        }

        private Request GetTasks(Request request)
        {
            var answer = new StringBuilder();
            var userId = request.State.UserId;
            var isInDataBase = dataBase.TryGetData(userId, idWithOffset, 0, default, out var tasksCountBytes);
            if (!isInDataBase || tasksCountBytes.ToInt() == 0)
            {
                request.Text = "У вас нет дел\n/Back";
                return request;
            }

            var tasks = new List<Tuple<DateTime, string>>();
            var tasksCount = tasksCountBytes.ToInt();
            for (var i = 1; i <= tasksCount; i++)
            {
                dataBase.TryGetData(userId, idWithOffset, i, default, out var task);
                tasks.Add(new Tuple<DateTime, string>(task.DateTimeFromFirstBytes(), task.ToStringFrom(8)));
            }

            var j = 1;
            foreach (var task in tasks.OrderBy(task => task.Item1))
            {
                answer.Append("/" + j + " - " + task.Item2 + " ( до " + task.Item1 + " )\n");
                j++;
            }

            answer.Append("/Back");
            request.Text = answer.ToString();
            return request;
        }

        private Request AnswerTasksViewing(Request request)
        {
            if (request.Text.ToLower() == "/back")
            {
                currentStates[request.State.UserId] = ThemeListState.Menu;
                request.Text = "";
                return MenuAnswer(request);
            }
            
            if (int.TryParse(request.Text.Substring(1), out var taskNumber))
            {
                var isInDataBase = dataBase.TryGetData(request.State.UserId, idWithOffset, 0, default,
                    out var tasksCountBytes);
                if (!isInDataBase || taskNumber > tasksCountBytes.ToInt())
                {
                    request.Text = "Задачи с таким номером нет\n/Back";
                    return request;
                }
            }

            return GetTasks(request);
        }

        private Request AnswerDeadlineAdding(Request request)
        {
            var userId = request.State.UserId;
            if (request.Text.ToLower() == "/back")
            {
                currentStates[userId] = ThemeListState.Menu;
                request.Text = "";
                return MenuAnswer(request);
            }
            if (DateTime.TryParse(request.Text, out var time))
            {
                newItems[userId] = new ListItem() {DeadLineTime = time};
                currentAddingStates[userId] = AddingStates.Text;
                request.Text = "Назовите дело\n/back";
                return request;
            }
            request.Text = "Неверный формат времени";
            return request;
        }

        private Request AnswerTaskTextAdding(Request request)
        {
            var userId = request.State.UserId;
            if (request.Text.ToLower() == "/back")
            {
                currentStates[userId] = ThemeListState.Menu;
                currentAddingStates[userId] = AddingStates.Deadline;
                request.Text = "/2";
                return MenuAnswer(request);
            }

            newItems[userId].Text = request.Text;
            SaveTask(newItems[userId], request.State);
            currentStates[userId] = ThemeListState.Menu;
            request.Text = "";
            return MenuAnswer(request);
        }

        private void SaveTask(ListItem item, State userState)
        {
            var isInDataBase = dataBase.TryGetData(userState.UserId, idWithOffset, 0, default, out var tasksCountBytes);
            if (!isInDataBase)
            {
                tasksCountBytes = 0.ToBytes();
            }

            var tasksCount = tasksCountBytes.ToInt();
            tasksCount++;
            var data = item.DeadLineTime.ToBytes().Concat(item.Text.ToBytes());
            dataBase.SaveData(userState.UserId, idWithOffset, tasksCount, default, data.ToArray());
            dataBase.SaveData(userState.UserId, idWithOffset, 0, default, tasksCount.ToBytes());
        }
        
    }
}