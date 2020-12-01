using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;

namespace Organizer
{
    public enum ThemeListState
    {
        Menu = 0,
        TasksViewing = 1,
        Adding = 2
    }

    public enum AddingStates
    {
        Deadline = 0,
        Text = 1
    }

    enum DeadLineTaskViewingStates
    {
        Listing,
        Concrete,
        TaskMenuChoosing,
        TaskChangingMenu,
        TaskDeadLineChanging,
        TaskTextChanging
    }
    
    public class DeadlineTasks : ThemeList
    {
        private Dictionary<int, AddingStates> currentAddingStates;
        private Dictionary<int, ListItem> newItems;
        private Dictionary<int, Func<int , int, UiRequest, Answer>> itemMenuAnswers;
        private Dictionary<int, DeadLineTaskViewingStates> currentViewingStates;
        private Dictionary<int, int> userInputedTaskNumbers;
        public override int GetId() => 1;
        protected override int GetIdWithOffset() => GetId() + 100;
        public override string GetName() => "Дела с дедлайном";

        protected override ListItem ParseListItemFromBytes(byte[] bytes)
        {
            return new ListItem()
            {
                DeadLineTime = bytes.DateTimeFromFirstBytes(),
                Text = bytes.ToStringFrom(8)
            };
        }

        protected override byte[] ConvertListItemToBytes(ListItem item)
        {
            return item.DeadLineTime.ToBytes().Concat(item.Text.ToBytes()).ToArray();
        }

        public DeadlineTasks(IDataBase dataBase) : base(dataBase)
        {
            currentViewingStates = new Dictionary<int, DeadLineTaskViewingStates>();
            currentAddingStates = new Dictionary<int, AddingStates>();
            newItems = new Dictionary<int, ListItem>();
            itemMenuAnswers = new Dictionary<int, Func<int, int, UiRequest, Answer>>()
            {
                [1] = TaskCompletedAnswer,
                [2] = TaskChangeAnswer
            };
            userInputedTaskNumbers = new Dictionary<int, int>();
        }

        protected override Answer ExistingTasksViewingAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!currentViewingStates.ContainsKey(userId))
            {
                currentViewingStates[userId] = DeadLineTaskViewingStates.Listing;
            }

            switch (currentViewingStates[userId])
            {
                case DeadLineTaskViewingStates.Listing:
                {
                    currentViewingStates[userId] = DeadLineTaskViewingStates.Concrete;
                    return TasksListAnswer(userId);
                }
                case DeadLineTaskViewingStates.Concrete:
                {
                    if (request.IsBackward)
                    {
                        currentViewingStates[userId] = DeadLineTaskViewingStates.Listing;
                        return Answer.BackWardAnswer(userId);
                    }

                    userInputedTaskNumbers[userId] = request.Number;
                    currentViewingStates[userId] = DeadLineTaskViewingStates.TaskMenuChoosing;
                    return TaskMenuAnswer(userId, userInputedTaskNumbers[userId]);
                }
                case DeadLineTaskViewingStates.TaskMenuChoosing:
                {
                    if (request.IsBackward)
                    {
                        currentViewingStates[userId] = DeadLineTaskViewingStates.Concrete;
                        return TasksListAnswer(userId);
                    }

                    return itemMenuAnswers[request.Number](userId, userInputedTaskNumbers[userId], request);
                }
                default:
                {
                    return TaskChangeAnswer(userId, userInputedTaskNumbers[userId], request);
                }
            }
        }

        private Answer TaskCompletedAnswer(int userId, int taskNumber, UiRequest request)
        {
            DeleteTask(userId, taskNumber - 1, default);
            currentViewingStates[userId] = DeadLineTaskViewingStates.Concrete;
            return TasksListAnswer(userId);
        }

        private Answer TaskChangeAnswer(int userId, int taskNumber, UiRequest request)
        {
            switch (currentViewingStates[userId])
            {
                case DeadLineTaskViewingStates.TaskMenuChoosing:
                {
                    currentViewingStates[userId] = DeadLineTaskViewingStates.TaskChangingMenu;
                    return TaskChangeMenuAnswer(userId, GetTask(userId, taskNumber, default));
                }
                case DeadLineTaskViewingStates.TaskChangingMenu:
                {
                    if (request.IsBackward)
                    {
                        currentViewingStates[userId] = DeadLineTaskViewingStates.TaskMenuChoosing;
                        return TaskMenuAnswer(userId, userInputedTaskNumbers[userId]);
                    }

                    currentViewingStates[userId] = request.Number == 1
                        ? DeadLineTaskViewingStates.TaskDeadLineChanging
                        : DeadLineTaskViewingStates.TaskTextChanging;
                    return ChangeRequestAnswer(userId);
                }
                default:
                {
                    if (request.IsBackward)
                    {
                        currentViewingStates[userId] = DeadLineTaskViewingStates.TaskChangingMenu;
                        return TaskChangeMenuAnswer(userId, GetTask(userId, taskNumber, default));
                    }

                    var taskToChange = GetTask(userId, taskNumber, default);
                    if (currentViewingStates[userId] == DeadLineTaskViewingStates.TaskTextChanging)
                    {
                        taskToChange.Text = request.Text;
                    }
                    else
                    {
                        taskToChange.DeadLineTime = request.DateTime;
                    }
                    
                    ChangeTask(userId, taskNumber, default, taskToChange);
                    currentViewingStates[userId] = DeadLineTaskViewingStates.TaskChangingMenu;
                    return TaskChangeMenuAnswer(userId, GetTask(userId, taskNumber, default));
                }
            }
        }

        private Answer ChangeRequestAnswer(int userId)
        {
            if (currentViewingStates[userId] == DeadLineTaskViewingStates.TaskTextChanging)
            {
                return Answer.AskForText(userId, "Введите новое название");
            }
            else
            {
                return Answer.AskForDate(userId);
            }
        }

        private Answer TaskChangeMenuAnswer(int userId, ListItem task)
        {
            return Answer.MenuAnswer(userId,
                string.Format("Изменение дела {0}", task.Text),
                new [] {"Изменить Время и дату", "Изменить имя"});
        }

        private Answer TaskMenuAnswer(int userId, int inputedTaskNumber)
        {
            var tasksCount = GetTasksCount(userId, default);
            var tasks = GetAllCurrentTasks(userId, tasksCount, default);
            var currentTask = tasks[inputedTaskNumber - 1];
            return Answer.MenuAnswer(userId, 
                string.Format("{0} (до {1})", new string[2] {currentTask.Text, currentTask.DeadLineTime.ToString()}),
                new string[] {"Дело сделано!", "Изменить дело"});
        }

        private Answer TasksListAnswer(int userId)
        {
            var tasksCount = GetTasksCount(userId, default);
            if (tasksCount == 0)
            {
                return Answer.EmptyListAnswer(userId, GetName());
            }

            var tasksLines = GetAllCurrentTasks(userId, tasksCount, default)
                .Select(task => task.Text + " (до " + task.DeadLineTime + ")");
            return Answer.ListAnswer(userId, GetName(), tasksLines.ToArray());
        }

        protected override Answer TaskAddingAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (request.IsShowThisItem)
            {
                currentAddingStates[userId] = AddingStates.Deadline;
                return Answer.AskForDateAndTime(userId);
            }

            switch (currentAddingStates[userId])
            {
                case AddingStates.Deadline:
                    if (request.IsBackward)
                    {
                        return Answer.BackWardAnswer(userId);
                    }
                    newItems[userId] = new ListItem(){ DeadLineTime = request.DateTime};
                    currentAddingStates[userId] = AddingStates.Text;
                    return Answer.AskForText(userId, "Назовите дело");
                case AddingStates.Text:
                    if (request.IsBackward)
                    {
                        currentAddingStates[userId] = AddingStates.Deadline;
                        return Answer.AskForText(userId, "Назовите дело");
                    }

                    newItems[userId].Text = request.Text;
                    SaveTask(newItems[userId], userId, default,
                        task => task.DeadLineTime.Ticks);
                    return Answer.BackWardAnswer(userId);
            }
            return Answer.BackWardAnswer(userId);
        }

        private void ChangeTask(int userId, int taskNumber, DateTime dateTime, ListItem item)
        {
            dataBase.SaveData(userId, GetIdWithOffset(), taskNumber, dateTime, ConvertListItemToBytes(item));
        }

        private void DeleteTask(int userId, int taskNumber, DateTime dateTime)
        {
            var tasksCount = GetTasksCount(userId, default);
            var tasks = GetAllCurrentTasks(userId, tasksCount, default);
            var taskList = tasks.ToList();
            taskList.RemoveAt(taskNumber);
            SaveTasks(userId, taskList.ToArray(), dateTime);
        }

        private void SaveTasks(int userId, ListItem[] tasks, DateTime dateTime)
        {
            var tasksCount = tasks.Length;
            for (var i = 1; i <= tasksCount; i++)
            {
                dataBase.SaveData(userId, GetIdWithOffset(), i, dateTime, ConvertListItemToBytes(tasks[i - 1]));
            }
            dataBase.SaveData(userId, GetIdWithOffset(), 0, dateTime, tasksCount.ToBytes());
            dataBase.RemoveData(userId, GetIdWithOffset(), tasksCount + 1, dateTime);
        }

        private ListItem GetTask(int userId, int taskNumber, DateTime dateTime)
        {
            var tasksCount = GetTasksCount(userId, dateTime);
            var allTasks = GetAllCurrentTasks(userId, tasksCount, dateTime);
            return allTasks[taskNumber - 1];
        }
    }
}