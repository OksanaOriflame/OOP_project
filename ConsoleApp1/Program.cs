using Ninject;
using Organizer.Application;
using Organizer.Application.Interfaces;
using Organizer.Infrastructure;
using Organizer.UI;

namespace Organizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = BuildContainer();

            var organizer = container.Get<Application.Organizer>();
            var tg = container.Get<TelegramBot>();

            tg.OnMessageRecieved += organizer.ProcessMessage;
            organizer.OnReplyRequest += tg.SendAnswer;

            organizer.Start();
        }

        private static StandardKernel BuildContainer()
        {
            var container = new StandardKernel();
            container.Bind<IUi>().To<TelegramBot>();
            container.Bind<IOrganizerItem>().To<ThemeLists>();
            container.Bind<IOrganizerItem>().To<AlarmOrganizerItem>();
            container.Bind<IDataBase>().To<DataBase>();
            container.Bind<ICorrectThemeList>().To<DeadlineList>();
            container.Bind<ICorrectThemeList>().To<DailyList>();
            return container;
        }

    }
}
