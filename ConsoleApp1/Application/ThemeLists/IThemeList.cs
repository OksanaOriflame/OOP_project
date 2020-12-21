namespace Organizer.Application
{
    public interface IThemeList
    {
        public int GetId();
        public string GetName();
        public Answer GetAnswer(UiRequest request, State userState);
    }

    public interface ICorrectThemeList
    {
        public int GetId();
        public string GetName();
        public ThemeListsMenu GetMenuTree(ThemeListsMenu root);
        public void ConnectDataBase(IDataBase dataBase);
    }
}