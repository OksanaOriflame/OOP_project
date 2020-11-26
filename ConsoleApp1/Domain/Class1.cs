using System.Collections.Generic;
using System.Linq;

namespace Organizer
{

    public class ThemeLists : IOrganizerItem
    {
        private List<IThemeList> themeLists;


        public int GetId() => 1;

        public string GetName() => "Тематические списки дел";

        public string GetMessage(RequestHandler requestHandler, string[] data)
        {
            return themeLists.FirstOrDefault().GetMessage(data);
        }

        RequestHandler IOrganizerItem.GetMessage(RequestHandler requestHandler)
        {
            throw new System.NotImplementedException();
        }

        public void Check()
        {
            throw new System.NotImplementedException();
        }

        public void Check(RequestHandler handler)
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IThemeList
    {
        public string GetMessage(string[] data);
    }

    public class DeadlineTasks : IThemeList
    {
        public string GetMessage(string[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}
