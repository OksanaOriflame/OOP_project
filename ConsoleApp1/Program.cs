using System;

namespace Organizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataBase = new DataBase();
            var tg = new TelegramBot(dataBase);
            var organizer = new Organizer(
                new IOrganizerItem[]
                {
                    new Alarm(dataBase), 
                    new ThemeLists(new IThemeList[]
                    {
                        new DeadlineTasks(dataBase),
                        new DailyTasks(dataBase) 
                    })
                },
                    dataBase,
                tg);
            //tg.OnMessageRecieved += organizer.ProcessMessage();
            //organizer.OnReplyRequest += tg.SendAnswer();
            Console.ReadKey();
        }
    }
}
