using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Organizer
{

    public class ThemeLists : IOrganizerItem
    {
        private Dictionary<int, IThemeList> themeLists;

        public ThemeLists(IThemeList[] lists)
        {
            themeLists = 
                new Dictionary<int, IThemeList>(
                    lists.Select(list => new KeyValuePair<int, IThemeList>(list.GetId(), list)));
        }

        public int GetId() => 1;

        public string GetName() => "Тематические списки дел";

        public Answer GetAnswer(UiRequest request, State userState)
        {
            if (userState.SubStateId == 0)
            {
                if (request.IsShowThisItem)
                {
                    return MenuAnswer(userState);
                }

                if (request.IsBackward)
                {
                    return Answer.BackWardAnswer(userState.UserId);
                }

                userState.SubStateId = request.Number;
                request.IsShowThisItem = true;
            }

            var answer = themeLists[userState.SubStateId].GetAnswer(request, userState);
            if (answer.IsBackward)
            {
                userState.SubStateId = 0;
                answer = MenuAnswer(userState);
            }

            return answer;
        }

        public CheckAnswer Check(int userId)
        {
            throw new NotImplementedException();
        }

        private Answer MenuAnswer(State userState)
        {
            return Answer.MenuAnswer(
                userState.UserId,
                "Списки дел",
                themeLists
                    .OrderBy(list => list.Key)
                    .Select(list => list.Value.GetName())
                    .ToArray());
        }
    }

    

    
}
