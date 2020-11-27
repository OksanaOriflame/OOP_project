using System;

namespace Organizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var organizer = new Organizer(
                new IOrganizerItem[]
                    {new ThemeLists()},
                    new DataBase(),
                new TelegramBot());
            var a = default(DateTime);
            Console.ReadKey();
        }
    }
}
