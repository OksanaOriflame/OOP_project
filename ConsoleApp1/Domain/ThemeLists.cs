using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Organizer
{

    public class ThemeLists : IOrganizerItem
    {
        private Dictionary<int, IThemeList> themeLists;
        private bool isActive;

        public ThemeLists(IThemeList[] lists)
        {
            themeLists = 
                new Dictionary<int, IThemeList>(
                    lists.Select(list => new KeyValuePair<int, IThemeList>(list.GetId(), list)));
            isActive = false;
        }

        public int GetId() => 1;

        public string GetName() => "Тематические списки дел";

        public Request GetMessage(Request request)
        {
            if (request.State.SubStateId == 0)
            {
                if (request.Text.ToLower() == "/back")
                {
                    isActive = false;
                    return new Request(new State(request.State.UserId, GlobalStates.Organizer, 0), default,
                        "Ok, vernulsya");
                }

                if (!isActive)
                {
                    isActive = true;
                    return MenuAnswer(request);
                }

                if (request.Text.Length > 1)
                {
                    var parseable = int.TryParse(request.Text.Substring(1), out var result);
                    if (parseable && themeLists.ContainsKey(result))
                    {
                        request.State.SubStateId = result;
                        return themeLists[result].GetAnswer(request);
                    }
                }

                return MenuAnswer(request);
            }

            var answer = themeLists[request.State.SubStateId].GetAnswer(request);
            if (answer.State.SubStateId == 0)
            {
                return MenuAnswer(answer);
            }

            return answer;
        }

        private Request MenuAnswer(Request request)
        {
            var answer = new StringBuilder();
            answer.Append("Выберите номер пункта меню::\n");
            foreach (var list in themeLists)
            {
                answer.Append("/" + list.Key + " - " + list.Value.GetName() + "\n");
            }

            answer.Append("/back");
            request.Text = answer.ToString();
            return request;
        }

        public void Check()
        {
            throw new System.NotImplementedException();
        }

        public void Check(Request handler)
        {
            throw new System.NotImplementedException();
        }
    }

    

    
}
