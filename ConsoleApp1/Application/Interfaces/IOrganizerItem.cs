namespace Organizer.Application
{
    public interface IOrganizerItem
    {
        public int GetId();
        public string GetName();
        public Answer GetAnswer(UiRequest request, State userState);
        public CheckAnswer Check(State userState);
        public void ConnectToDataBase(IDataBase dataBase);
    }
}
