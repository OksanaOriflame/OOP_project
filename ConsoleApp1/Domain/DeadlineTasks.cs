using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Ocsp;

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
        TaskMenuChoosing
    }
    
    public class DeadlineTasks : ThemeList, IThemeList
    {
        private Dictionary<int, AddingStates> currentAddingStates;
        private Dictionary<int, ListItem> newItems;
        private Dictionary<int, DeadLineTaskViewingStates> currentViewingStates;
        public int GetId() => 1;
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
        }

        protected override Answer AnswerExistingTasksViewing(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!currentViewingStates.ContainsKey(userId))
            {
                currentViewingStates[userId] = DeadLineTaskViewingStates.Listing;
            }

            if (currentViewingStates[userId] == DeadLineTaskViewingStates.Listing)
            {
                currentViewingStates[userId] = DeadLineTaskViewingStates.Concrete;
                return TasksListAnswer(userId);
            }
            else if (currentViewingStates[userId] == DeadLineTaskViewingStates.Concrete)
            {
                if (request.IsBackward)
                {
                    return Answer.BackWardAnswer(userId);
                }

                var inputedTaskNumber = request.Number;
                currentViewingStates[userId] = DeadLineTaskViewingStates.TaskMenuChoosing;
                return TaskMenuAnswer(userId, inputedTaskNumber);
            }
            else if (currentViewingStates[userId] == DeadLineTaskViewingStates.TaskMenuChoosing)
            {
                if (request.IsBackward)
                {
                    currentViewingStates[userId] = DeadLineTaskViewingStates.Concrete;
                    return TasksListAnswer(userId);
                }
            }

            currentViewingStates[userId] = DeadLineTaskViewingStates.Listing;
            return Answer.BackWardAnswer(userId);
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

        protected override Answer AnswerTaskAdding(UiRequest request, State userState)
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
                    SaveTask(newItems[userId], userState, default,
                        item => item.DeadLineTime.Ticks);
                    return Answer.BackWardAnswer(userId);
            }
            return Answer.BackWardAnswer(userId);
        }

    }
}