namespace Organizer
{
    public interface IThemeList
    {
        public int GetId();
        public string GetName();
        public Answer GetAnswer(UiRequest request, State userState);
    }
}