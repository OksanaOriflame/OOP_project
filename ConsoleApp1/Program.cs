using System;

namespace Organizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataBase = new DataBase();
            var organizer = new Organizer(
                new IOrganizerItem[]
                {
                    new ThemeLists(new IThemeList[]
                    {
                        new DeadlineTasks(dataBase),
                        new DailyTasks(dataBase) 
                    })
                },
                    dataBase,
                new TelegramBot(dataBase));
            Console.ReadKey();
        }
    }
}
