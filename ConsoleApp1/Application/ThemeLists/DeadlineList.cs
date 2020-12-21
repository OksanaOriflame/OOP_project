using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Bcpg;
using Organizer.Infrastructure;

namespace Organizer.Application
{
    public class DeadlineList : ICorrectThemeList
    {
        public int GetId() => 101;
        public string GetName() => "Дела с дедлайном";
        private Dictionary<int, DeadlineTasks> usersLists;
        private Dictionary<int, int> usersCurrentListItems;
        private Dictionary<int, string> usersNewTaskNames;
        private ThemeListsMenu mainMenu;
        private IDataBase dataBase;

        

        public DeadlineList()
        {
            usersLists = new Dictionary<int, DeadlineTasks>();
            usersCurrentListItems = new Dictionary<int, int>();
            usersNewTaskNames = new Dictionary<int, string>();
        }

        public ThemeListsMenu GetMenuTree(ThemeListsMenu root)
        {
            var addTaskSecondMenu = new ThemeListsMenu()
            {
                MenuName = "Введите дату и время дедлайна",
                MenuType = MenuType.RequestForData,
                ProcessData = ProcessNewTaskDateTime,
                RequestFormat = ExpectingRequestFormat.DateTime,
                Options = null
            };

            var addTaskFirstMenu = new ThemeListsMenu()
            {
                MenuName = "Добавление нового дела",
                MenuType = MenuType.RequestForData,
                ProcessData = ProcessNewTaskName,
                RequestFormat = ExpectingRequestFormat.Text,
                Options = null
            };

            var tasksListMenu = new ThemeListsMenu()
            {
                MenuName = "Ваши дела с дедлайном",
                MenuType = MenuType.Menu,
                Options = null,
                ProcessData = ChooseListItemProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            var mainMenu = new ThemeListsMenu()
            {
                MenuName = "Дела с дедлайном",
                MenuType = MenuType.Menu,
                Options = new List<ThemeListsMenu>(){ tasksListMenu, addTaskFirstMenu },
                PreviousMenu = root,
                ProcessData = MenuProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            addTaskSecondMenu.PreviousMenu = addTaskFirstMenu;
            addTaskSecondMenu.Options = new List<ThemeListsMenu>(){mainMenu};

            addTaskFirstMenu.PreviousMenu = mainMenu;
            addTaskFirstMenu.Options = new List<ThemeListsMenu>(){addTaskSecondMenu};

            tasksListMenu.PreviousMenu = mainMenu;

            this.mainMenu = mainMenu;
            return mainMenu;
        }

        private void MenuProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (request.IsBackward)
            {
                if (usersLists.ContainsKey(userId))
                {
                    if (usersLists[userId] != null)
                    {
                        SaveAllTasks(userId, usersLists[userId].GetAllTasksTuples().ToList());
                    }
                }
                usersLists[userId] = null;
            }
            else
            {
                usersLists[userId] = new DeadlineTasks(GetAllTasks(userId).ToArray());
                var tasks = usersLists[userId].GetAllTasks();
                var tasksOptions = tasks
                    .Select(t => new ThemeListsMenu()
                    {
                        MenuName = "Дело сделано!",
                        MenuType = MenuType.Menu,
                        Options = new List<ThemeListsMenu>
                        {

                        },
                        PreviousMenu = null,
                        ProcessData = null,
                        RequestFormat = ExpectingRequestFormat.Number
                    })
                    .ToList();
                mainMenu.Options[0].Options = tasks
                    .Select((task, i) =>
                    {
                        var itemMenu = new ThemeListsMenu
                        {
                            MenuName = task,
                            MenuType = MenuType.Menu,
                            Options = new List<ThemeListsMenu> {tasksOptions[i]},
                            PreviousMenu = mainMenu.Options[0],
                            ProcessData = ListItemProcessData,
                            RequestFormat = ExpectingRequestFormat.Number
                        };
                        tasksOptions[i].PreviousMenu = itemMenu;
                        return itemMenu;
                    })
                    .ToList();
            }
        }

        private void ListItemProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            if (request.Number == 1)
            {
                usersLists[request.UserId].DeleteTask(usersCurrentListItems[request.UserId] - 1);
                SaveAllTasks(request.UserId, usersLists[request.UserId].GetAllTasksTuples().ToList());
                mainMenu.Options[0].Options[usersCurrentListItems[request.UserId] - 1].Options[0] = mainMenu;
            }
        }

        private void ChooseListItemProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            if (!request.IsBackward)
            {
                usersCurrentListItems[request.UserId] = request.Number;
            }
        }

        private List<Tuple<DateTime, string>> GetAllTasks(int userId)
        {
            var isInDataBase = dataBase.TryGetData(userId, GetId(), 0, default, out var tasksCountBytes);
            if (!isInDataBase)
            {
                SaveAllTasks(userId, new List<Tuple<DateTime, string>>());
                return new List<Tuple<DateTime, string>>();
            }

            var tasksCount = tasksCountBytes.ToInt();
            var result = new List<Tuple<DateTime, string>>();
            for (var i = 1; i <= tasksCount; i++)
            {
                dataBase.TryGetData(userId, GetId(), i, default, out var taskBytes);
                result.Add(BytesExtensions.ParseDateAndNameFromBytes(taskBytes));
            }

            return result;
        }

        private void SaveAllTasks(int userId, List<Tuple<DateTime, string>> tasks)
        {
            var tasksCount = tasks.Count;
            dataBase.SaveData(userId, GetId(), 0, default, tasksCount.ToBytes());
            var i = 1;
            foreach(var task in tasks)
            {
                dataBase.SaveData(userId, GetId(), i, default, BytesExtensions.ConvertDateAndNameToBytes(task.Item1, task.Item2));
                i++;
            }
        }

        private void ProcessNewTaskDateTime(ThemeListsMenu currentMenu, UiRequest request)
        {
            if (!request.IsBackward)
            {
                var userId = request.UserId;
                usersLists[userId].AddTask(usersNewTaskNames[userId], request.DateTime);
                SaveAllTasks(userId, usersLists[userId].GetAllTasksTuples().ToList());
            }
        }

        private void ProcessNewTaskName(ThemeListsMenu currentMenu, UiRequest request)
        {
            if (!request.IsBackward)
            {
                usersNewTaskNames[request.UserId] = request.Text;
            }
        }

        public void ConnectDataBase(IDataBase dataBase)
        {
            this.dataBase = dataBase;
        }
    }
}
