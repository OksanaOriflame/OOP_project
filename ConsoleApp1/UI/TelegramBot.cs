using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Organizer
{
    public class TelegramBot : IUi
    {
        private Dictionary<long, long> chatIds;
        private TelegramBotClient bot;
        private Dictionary<int, Answer> answers;
        private IDataBase dataBase;
        public event Action<UiRequest> OnMessageRecieved;

        public TelegramBot(IDataBase dataBase)
        {
            chatIds = new Dictionary<long, long>();
            bot = new TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
            bot.OnMessage += ReadMessage;
            bot.StartReceiving();
            this.dataBase = dataBase;
            answers = new Dictionary<int, Answer>();
        }

        private void ReadMessage(object sender, MessageEventArgs args)
        {
            var userId = args.Message.From.Id;
            var chatId = args.Message.Chat.Id;
            var message = args.Message.Text;
            chatIds[userId] = chatId;
            if (message == null)
            {
                bot.SendTextMessageAsync(chatId, "Я не понимаю. Напиши словами, плес");
            }
            else
            {
                var request = new UiRequest() {UserId = userId};
                var lastAnswer = GetLastAnswer(userId);
                if (GetStringWithoutOpeningSlash(message).ToLower() == "back")
                {
                    request.IsBackward = true;
                    OnMessageRecieved!(request);
                    return;
                }
                switch (lastAnswer.Format)
                {
                    case ExpectingRequestFormat.FirstRequest:
                        request.IsShowThisItem = true;
                        break;
                    case ExpectingRequestFormat.Text:
                        request.Text = message;
                        break;
                    case ExpectingRequestFormat.DateTime :
                    {
                        var isDateTime = DateTime.TryParse(message, out var dateTime);
                        if (!isDateTime)
                        {
                            bot.SendTextMessageAsync(chatId,
                                "Некорректный формат даты.\nПопробуйте ДД-ММ-ГГГГ ЧЧ:ММ\n/Back");
                            return;
                        }

                        request.DateTime = dateTime;
                        break;
                    }
                    case ExpectingRequestFormat.OnlyDayDateTime:
                    {
                        var isDateTime = DateTime.TryParse(message, out var dateTime);
                        if (!isDateTime)
                        {
                            bot.SendTextMessageAsync(chatId,
                                "Некорректный формат даты.\nПопробуйте ДД-ММ-ГГГГ ЧЧ:ММ\n/Back");
                            return;
                        }
                        
                        request.DateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                        break;
                    }
                    case ExpectingRequestFormat.Number:
                        message = GetStringWithoutOpeningSlash(message);
                        var isInt = int.TryParse(message, out var number);
                        if (!isInt || number < lastAnswer.NumberRange.Item1 || number > lastAnswer.NumberRange.Item2)
                        {
                            bot.SendTextMessageAsync(chatId, "Неверное число\n/Back");
                            return;
                        }

                        request.Number = number;
                        break;
                }
                OnMessageRecieved!(request);
            }
        }

        private string GetStringWithoutOpeningSlash(string str)
        {
            if (str.Length > 1 && str[0] == '/')
            {
                return str.Substring(1);
            }

            return str;
        }

        private Answer GetLastAnswer(int userId)
        {
            if (!answers.ContainsKey(userId))
            {
                var answer = new Answer()
                {
                    Format = ExpectingRequestFormat.FirstRequest, NumberRange = new Tuple<int, int>(0, 0)
                };
                answers[userId] = answer;
            }
            return answers[userId];
        }

        public void SendAnswer(Answer answer)
        {
            var answerText = new StringBuilder();
            answerText.Append(answer.Headline + "\n");
            var i = 1;
            foreach (var answerItem in answer.Items)
            {
                answerText.Append("/" + i + " - " + answerItem + "\n");
                i++;
            }

            answerText.Append("/Back");
            answers[answer.UserId] = answer;
            bot.SendTextMessageAsync(chatIds[answer.UserId], answerText.ToString());
        }

        public void SendCheckAnswer(CheckAnswer[] answers)
        {
            var message = new StringBuilder();
            foreach (var answer in answers)
            {
                message.Append(answer.HeadLine);
                foreach (var item in answer.Items)
                {
                    message.Append("\t" + item);
                }
            }
        }
    }
}
