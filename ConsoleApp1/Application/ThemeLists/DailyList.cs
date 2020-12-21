using Organizer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Organizer.Application
{
    public class DailyList : ICorrectThemeList
    {
        private IDataBase dataBase;
        private Dictionary<int, DailyTasks> usersDailyTasks;
        private Dictionary<int, DateTime> usersCurrentDateTimes;
        private Dictionary<int, int> usersCurrentTaskNumbers;
        private ThemeListsMenu mainMenu;
        public string GetName() => "ежедневные дела";
        public int GetId() => 1000;

        public DailyList()
        {
            usersDailyTasks = new Dictionary<int, DailyTasks>();
            usersCurrentDateTimes = new Dictionary<int, DateTime>();
            usersCurrentTaskNumbers = new Dictionary<int, int>();
        }

        public void ConnectDataBase(IDataBase dataBase)
        {
            this.dataBase = dataBase;
        }

        public ThemeListsMenu GetMenuTree(ThemeListsMenu root)
        {
            mainMenu = new ThemeListsMenu()
            {
                MenuName = "ежедневные дела",
                MenuType = MenuType.Menu,
                Options = null,
                PreviousMenu = root,
                ProcessData = MainMenuProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            var watchTasksMenu = new ThemeListsMenu()
            {
                MenuName = "Ваши дела по датам",
                MenuType = MenuType.Menu,
                Options = null,
                PreviousMenu = mainMenu,
                ProcessData = ChooseTaskToWatchProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            var addNewTaskMenu = new ThemeListsMenu()
            {
                MenuName = "Добавить новое дело",
                MenuType = MenuType.Menu,
                Options = null,
                PreviousMenu = mainMenu,
                ProcessData = AddNewTaskProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            var addNewDateMenu = new ThemeListsMenu()
            {
                MenuName = "Добавить новую дату",
                MenuType = MenuType.RequestForData,
                Options = new List<ThemeListsMenu>() {addNewTaskMenu},
                PreviousMenu = addNewTaskMenu,
                ProcessData = AddNewDateProcessData,
                RequestFormat = ExpectingRequestFormat.OnlyDayDateTime
            };

            var addNewTaskOnExistingDateMenu = new ThemeListsMenu()
            {
                MenuName = "Добавить новое дело на существующую дату",
                MenuType = MenuType.Menu,
                Options = null,
                PreviousMenu = addNewTaskMenu,
                ProcessData = AddNewTaskOnExistingDateProcessData,
                RequestFormat = ExpectingRequestFormat.Number
            };

            addNewTaskMenu.Options = new List<ThemeListsMenu> {addNewDateMenu, addNewTaskOnExistingDateMenu};

            mainMenu.Options = new List<ThemeListsMenu> {watchTasksMenu, addNewTaskMenu};

            return mainMenu;
        }

        private void MainMenuProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (request.IsBackward)
            {
                usersDailyTasks[userId] = null;
            }
            else if (!usersDailyTasks.ContainsKey(userId) || usersDailyTasks[userId] == null)
            {
                var datesCount = GetUsersDateTimesCount(userId);
                var dates = GetUsersDateTimes(userId, datesCount);
                var tasksOnDates = dates
                    .Select(d =>
                    {
                        var tasksCount = GetUsersTasksCountByDate(userId, d);
                        return Tuple.Create(d, GetAllTasksOnDate(userId, d, tasksCount));
                    })
                    .ToList();
                usersDailyTasks[userId] = new DailyTasks(tasksOnDates);
                mainMenu.Options[0].Options = usersDailyTasks[userId]
                    .GetAllDateTimes()
                    .Select(d => new ThemeListsMenu()
                    {
                        MenuName = d.ToString(),
                        MenuType = MenuType.Menu,
                        Options = null,
                        PreviousMenu = mainMenu.Options[0],
                        ProcessData = ChooseTaskNumberProcessData,
                        RequestFormat = ExpectingRequestFormat.Number
                    })
                    .ToList();
            }
        }

        private void ChooseTaskToWatchProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                usersCurrentDateTimes[userId] = usersDailyTasks[userId].GetAllDateTimes()[request.Number - 1];

                currentMenu.Options[request.Number - 1].Options = usersDailyTasks[userId]
                    .GetAllTasksOnDate(usersCurrentDateTimes[userId])
                    .Select(t => new ThemeListsMenu()
                    {
                        MenuName = t,
                        MenuType = MenuType.Menu,
                        Options = new List<ThemeListsMenu>
                        {
                            new ThemeListsMenu()
                            {
                                MenuName = "Дело сделано!",
                            },
                            new ThemeListsMenu()
                            {
                                MenuName = "Изменить название дела",
                                MenuType = MenuType.RequestForData,
                                Options = new List<ThemeListsMenu> {mainMenu},
                                PreviousMenu = currentMenu,
                                ProcessData = TaskNameChangeProcessData,
                                RequestFormat = ExpectingRequestFormat.Text
                            }
                        },
                        PreviousMenu = currentMenu.Options[request.Number - 1],
                        ProcessData = TaskNumberProcessData,
                        RequestFormat = ExpectingRequestFormat.Number
                    })
                    .ToList();
            }
        }

        private void ChooseTaskNumberProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            if (!request.IsBackward)
                usersCurrentTaskNumbers[request.UserId] = request.Number;
        }

        private void TaskNumberProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                if (request.Number == 1)
                {
                    var date = usersCurrentDateTimes[userId];
                    usersDailyTasks[userId].DeleteTask(date, usersCurrentTaskNumbers[userId] - 1);
                    SaveTasksOnDate(userId, date, usersDailyTasks[userId].GetAllTasksOnDate(date));
                    currentMenu.Options[0] = mainMenu;
                }
            }
        }

        private void TaskNameChangeProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                var date = usersCurrentDateTimes[userId];
                usersDailyTasks[userId].ChangeTaskName(date, usersCurrentTaskNumbers[userId] - 1, request.Text);
                SaveTasksOnDate(userId, date, usersDailyTasks[userId].GetAllTasksOnDate(date));
            }
        }

        private void AddNewTaskProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                if (request.Number == 2)
                {
                    var dates = usersDailyTasks[userId].GetAllDateTimes();
                    currentMenu.Options[1].Options = dates
                        .Select(d => new ThemeListsMenu()
                        {
                            MenuName = d.ToString(),
                            MenuType = MenuType.RequestForData,
                            Options = new List<ThemeListsMenu> {currentMenu},
                            PreviousMenu = currentMenu,
                            ProcessData = AddNewTaskOnExistingDateProcessName,
                            RequestFormat = ExpectingRequestFormat.Text
                        })
                        .ToList();
                }
            }
        }

        private void AddNewDateProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                var inputedDay = request.DateTime;
                var newDate = new DateTime(inputedDay.Year, inputedDay.Month, inputedDay.Day);
                if (usersDailyTasks[userId].GetAllDateTimes().Contains(newDate))
                {
                    currentMenu.MenuName = "Такая дата уже существует";
                    currentMenu.Options = new List<ThemeListsMenu> {currentMenu};
                }
                else
                {
                    currentMenu.MenuName = "Добавить новую дату";
                    currentMenu.Options = new List<ThemeListsMenu> {currentMenu.PreviousMenu};
                    usersDailyTasks[userId].AddNewDate(newDate);
                    SaveDates(userId, usersDailyTasks[userId].GetAllDateTimes());
                }
            }
            else
            {
                currentMenu.MenuName = "Добавить новую дату";
                currentMenu.Options = new List<ThemeListsMenu> { currentMenu.PreviousMenu };
            }
        }

        private void AddNewTaskOnExistingDateProcessData(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                usersCurrentDateTimes[userId] = usersDailyTasks[userId].GetAllDateTimes()[request.Number - 1];
            }
        }

        private void AddNewTaskOnExistingDateProcessName(ThemeListsMenu currentMenu, UiRequest request)
        {
            var userId = request.UserId;
            if (!request.IsBackward)
            {
                if (usersDailyTasks[userId].GetAllTasksOnDate(usersCurrentDateTimes[userId]).Contains(request.Text))
                {
                    currentMenu.MenuName = "Такое дело уже существует, введите другое";
                    currentMenu.Options = new List<ThemeListsMenu>() {currentMenu};
                }
                else
                {
                    currentMenu.MenuName = "Введите название дела";
                    currentMenu.Options = new List<ThemeListsMenu>() { currentMenu.PreviousMenu };
                    usersDailyTasks[userId].AddNewTask(usersCurrentDateTimes[userId], request.Text);
                    SaveTasksOnDate(userId, usersCurrentDateTimes[userId], usersDailyTasks[userId].GetAllTasksOnDate(usersCurrentDateTimes[userId]));
                }
            }
            else
            {
                currentMenu.MenuName = "Введите название дела";
                currentMenu.Options = new List<ThemeListsMenu>() { currentMenu.PreviousMenu };
            }
        }

        private int GetUsersDateTimesCount(int userId)
        {
            var isInDataBase = dataBase.TryGetData(userId, GetId(), 0, default, out var datesCountBytes);
            if (!isInDataBase)
            {
                datesCountBytes = 0.ToBytes();
                dataBase.SaveData(userId, GetId(), 0, default, datesCountBytes);
            }

            return datesCountBytes.ToInt();
        }

        private List<DateTime> GetUsersDateTimes(int userId, int count)
        {
            if (count == 0)
                return new List<DateTime>();
            dataBase.TryGetData(userId, GetId(), 1, default, out var datesBytes);
            return datesBytes.ToDateTimesList(count);
        }

        private int GetUsersTasksCountByDate(int userId, DateTime date)
        {
            var isInDataBase = dataBase.TryGetData(userId, GetId(), 0, date, out var tasksCountBytes);
            if (!isInDataBase)
            {
                tasksCountBytes = 0.ToBytes();
                dataBase.SaveData(userId, GetId(), 0, date, tasksCountBytes);
            }

            return tasksCountBytes.ToInt();
        }

        private List<string> GetAllTasksOnDate(int userId, DateTime date, int tasksCount)
        {
            if (tasksCount == 0)
                return new List<string>();
            var result = new List<string>();
            for (var i = 1; i <= tasksCount; i++)
            {
                dataBase.TryGetData(userId, GetId(), i, date, out var taskBytes);
                result.Add(taskBytes.To_String());
            }

            return result;
        }

        private void SaveDates(int userId, List<DateTime> dates)
        {
            dataBase.SaveData(userId, GetId(), 0, default, dates.Count.ToBytes());
            dataBase.SaveData(userId, GetId(), 1, default, dates.ToBytes());
        }

        private void SaveTasksOnDate(int userId, DateTime date, List<string> tasks)
        {
            dataBase.SaveData(userId, GetId(), 0, date, tasks.Count.ToBytes());
            for (var i = 0; i < tasks.Count; i++)
            {
                dataBase.SaveData(userId, GetId(), i + 1, date, tasks[i].ToBytes());
            }
        }
    }
}