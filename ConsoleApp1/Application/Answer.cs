using System;

namespace Organizer
{
    public enum ExpectingRequestFormat
    {
        Text,
        DateTime,
        OnlyDayDateTime,
        OnlyTimeDateTime,
        Number,
        FirstRequest
    }
    
    public class Answer
    {
        public int UserId { get; set; }
        public string Headline { get; set; }
        public string[] Items { get; set; } 
        public ExpectingRequestFormat Format { get; set; }
        public Tuple<int, int> NumberRange { get; set; }
        public bool IsBackward { get; set; }

        public static Answer BackWardAnswer(int userId)
        {
            return new Answer()
            {
                IsBackward = true,
                UserId = userId
            };
        }

        public static Answer MenuAnswer(int userId, string headline, string[] menuItems)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.Number,
                IsBackward = false,
                Headline = headline + "\nВыберите пункт меню:",
                Items = menuItems,
                NumberRange = new Tuple<int, int>(1, menuItems.Length)
            };
        }

        public static Answer EmptyListAnswer(int userId, string headLine)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.Number,
                Headline = headLine + "\n" + "Список Пуст",
                IsBackward = false,
                Items = new string[0],
                NumberRange = new Tuple<int, int>(0, 0)
            };
        }

        public static Answer ListAnswer(int userId, string headLine, string[] items)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.Number,
                Headline = headLine + "\nВыберите нужный элемент списка:",
                IsBackward = false,
                Items = items,
                NumberRange = new Tuple<int, int>(1, items.Length)
            };
        }

        public static Answer AskForDateAndTime(int userId)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.DateTime,
                IsBackward = false,
                Headline = "Введите Дату и время",
                Items = new string[0]
            };
        }
        
        public static Answer AskForDate(int userId)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.OnlyDayDateTime,
                IsBackward = false,
                Headline = "Введите Дату",
                Items = new string[0]
            };
        }

        public static Answer AskForTime(int userId)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.OnlyTimeDateTime,
                IsBackward = false,
                Headline = "Введите время",
                Items = new string[0]
            };
        }

        public static Answer DateIsAlreadyExists(int userId)
        {
            var answer = AskForDate(userId);
            answer.Headline = "Такая дата уже существует - введите другую";
            return answer;
        }

        public static Answer AskForText(int userId, string headline)
        {
            return new Answer()
            {
                UserId = userId,
                Format = ExpectingRequestFormat.Text,
                Headline = headline,
                IsBackward = false,
                Items = new string[0]
            };
        }
    }
}