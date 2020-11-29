namespace Organizer
{
    public interface IThemeList
    {
        public int GetId();
        public string GetName();
        public Request GetAnswer(Request request);
    }
}