﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Organizer
{
    public abstract class ThemeList
    {
        public abstract string GetName();
        protected abstract int GetIdWithOffset();
        protected IDataBase dataBase;
        protected Dictionary<int, Func<UiRequest, State, Answer>> menuActions;
        protected Dictionary<int, ThemeListState> currentStates;
        protected abstract byte[] ConvertListItemToBytes(ListItem item);
        protected abstract ListItem ParseListItemFromBytes(byte[] bytes);

        public ThemeList(IDataBase dataBase)
        {
            this.dataBase = dataBase;
            menuActions = new Dictionary<int, Func<UiRequest, State, Answer>>
            {
                [1] = AnswerTasksViewing, [2] = AnswerTaskAdding
            };
            currentStates = new Dictionary<int, ThemeListState>();
        }
        
        public Answer GetAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!currentStates.ContainsKey(userId))
            {
                currentStates[userId] = ThemeListState.Menu;
            }

            var currentState = currentStates[userId];
            switch (currentState)
            {
                case ThemeListState.Menu:
                {
                    if (request.IsBackward)
                    {
                        return Answer.BackWardAnswer(userId);
                    }

                    if (request.IsShowThisItem)
                    {
                        return MenuAnswer(userState);
                    }

                    request.IsShowThisItem = true;
                    currentStates[userId] = (ThemeListState) request.Number;
                    var answer = menuActions[request.Number](request, userState);
                    if (answer.IsBackward)
                    {
                        answer = MenuAnswer(userState);
                        currentStates[userId] = ThemeListState.Menu;
                    }

                    return answer;
                }
                default:
                {
                    var answer = menuActions[(int)currentStates[userId]](request, userState);
                    if (answer.IsBackward)
                    {
                        currentStates[userId] = ThemeListState.Menu;
                        answer = MenuAnswer(userState);
                    }

                    return answer;
                }
            }
        }
        
        protected Answer AnswerTasksViewing(UiRequest request, State userState)
        {
            return AnswerExistingTasksViewing(request, userState);
        }

        protected abstract Answer AnswerTaskAdding(UiRequest request, State userState);
        
        internal Answer MenuAnswer(State userState)
        {
            return Answer.MenuAnswer(
                userState.UserId, 
                GetName(), 
                new [] {"Посмотреть дела", "Добавить новое дело"});
        }
        
        protected void SaveTask(ListItem item, State userState, DateTime dateTime, Func<ListItem, long> orderSelector)
        {
            var tasksCount = GetTasksCount(userState.UserId, dateTime);
            var allTasksArray = GetAllCurrentTasks(userState.UserId, tasksCount, dateTime);
            var allTasks = allTasksArray.ToList();
            allTasks.Add(item);
            var i = 1;
            tasksCount++;
            foreach (var task in allTasks
                .OrderBy(orderSelector)
                .Select(ConvertListItemToBytes))
            {
                dataBase.SaveData(userState.UserId, GetIdWithOffset(), i, dateTime, task.ToArray());
                i++;
            }
            dataBase.SaveData(userState.UserId, GetIdWithOffset(), 0, dateTime, tasksCount.ToBytes());
        }
        protected int GetTasksCount(int userId, DateTime dateTime)
        {
            var isInDataBase = dataBase.TryGetData(userId, GetIdWithOffset(), 0, dateTime,
                out var tasksCountBytes);
            if (!isInDataBase)
            {
                dataBase.SaveData(userId, GetIdWithOffset(), 0, dateTime, 0.ToBytes());
                tasksCountBytes = 0.ToBytes();
            }

            return tasksCountBytes.ToInt();
        }

        protected ListItem[] GetAllCurrentTasks(int userId, int tasksCount, DateTime dateTime)
        {
            if (tasksCount == 0)
            {
                return new ListItem[0];
            }
            var items = new List<ListItem>();
            for (var i = 1; i <= tasksCount; i++)
            {
                dataBase.TryGetData(userId, GetIdWithOffset(), i, dateTime, out var taskBytes);
                items.Add(ParseListItemFromBytes(taskBytes));
            }

            return items.ToArray();
        }

        protected abstract Answer AnswerExistingTasksViewing(UiRequest request, State userState);
    }
}