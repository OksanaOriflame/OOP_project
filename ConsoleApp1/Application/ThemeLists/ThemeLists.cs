using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Organizer.Infrastructure;

namespace Organizer.Application
{
    public class ThemeLists : IOrganizerItem
    {
        public int GetId() => 1;

        public string GetName() => "Списки дел";
        private Dictionary<int, ThemeListsMenu> usersMenu;
        private ThemeListsMenu initialMenu;
        private List<ICorrectThemeList> lists;
        private IDataBase dataBase;

        public ThemeLists(List<ICorrectThemeList> lists)
        {
            this.lists = lists;
            usersMenu = new Dictionary<int, ThemeListsMenu>();
            initialMenu = new ThemeListsMenu()
            {
                MenuName = GetName(),
                Options = new List<ThemeListsMenu>(),
                PreviousMenu = null,
                MenuType = MenuType.Menu,
                RequestFormat = ExpectingRequestFormat.Number
            };
            BuildMenuTree();
        }

        public void ConnectToDataBase(IDataBase dataBase)
        {
            this.dataBase = dataBase;
            foreach (var list in lists)
            {
                list.ConnectDataBase(dataBase);
            }
        }

        public Answer GetAnswer(UiRequest request, State userState)
        {
            var userId = userState.UserId;
            if (!usersMenu.ContainsKey(userId) || request.IsShowThisItem)
            {
                usersMenu[userId] = initialMenu;
                return initialMenu.GetAnswer(userId);
            }
            usersMenu[userId] = usersMenu[userId].GetNextMenu(request);
            if (usersMenu[userId] == null)
                return Answer.BackWardAnswer(userId);
            return usersMenu[userId].GetAnswer(userId);
        }

        public CheckAnswer Check(State userState)
        {
            throw new NotImplementedException();
        }
        
        private void BuildMenuTree()
        {
            foreach (var list in lists)
            {
                initialMenu.Options.Add(list.GetMenuTree(initialMenu));
            }
        }
    }

    public enum MenuType
    {
        Menu,
        RequestForData
    }
    
    public class ThemeListsMenu
    {
        public string MenuName { get; set; }
        public ThemeListsMenu PreviousMenu { get; set; }
        public List<ThemeListsMenu> Options { get; set; }
        public ExpectingRequestFormat RequestFormat { get; set; }
        public Action<ThemeListsMenu, UiRequest> ProcessData { get; set; }
        public MenuType MenuType { get; set; }

        public Answer GetAnswer(int userId)
        {
            if (MenuType == MenuType.Menu)
                return MenuGetAnswer(userId);
            return RequestForDataAnswer(userId);
        }

        public ThemeListsMenu GetNextMenu(UiRequest request)
        {
            ProcessData?.Invoke(this, request);
            if (request.IsBackward)
                return PreviousMenu;
            if (MenuType == MenuType.Menu)
                return Options[request.Number - 1];
            return Options[0];
        }

        private Answer MenuGetAnswer(int userId)
        {
            if (Options.Count == 0)
                return Answer.EmptyListAnswer(userId, MenuName);
            return Answer.ListAnswer(userId, MenuName, Options.Select(menu => menu.MenuName).ToArray());
        }

        private Answer RequestForDataAnswer(int userId)
        {
            switch (RequestFormat)
            {
                case ExpectingRequestFormat.DateTime:
                    return Answer.AskForDate(userId);
                case ExpectingRequestFormat.Text:
                    return Answer.AskForText(userId, "Введите текст");
                case ExpectingRequestFormat.OnlyDayDateTime:
                    return Answer.AskForDate(userId);
                default:
                    return Answer.EmptyListAnswer(userId, MenuName);
            }
        }
    }
}
