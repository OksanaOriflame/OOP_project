using System;
using System.Collections.Generic;
using System.Text;
using Organizer.Application.Interfaces;
using Organizer.Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Organizer.UI
{
    public class TelegramBot : IUi
    {
        private Dictionary<long, long> chatIds;
        private TelegramBotClient bot;
        private Dictionary<int, ExpectingRequestFormat> usersExpectingFormats;
        private Dictionary<int, Tuple<int, int>> usersRanges;
        public event Action<int, DateTime, string, int, bool, bool> OnMessageRecieved;

        public TelegramBot()
        {
            chatIds = new Dictionary<long, long>();
            bot = new TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
            bot.OnMessage += ReadMessage;
            bot.StartReceiving();
            usersExpectingFormats = new Dictionary<int, ExpectingRequestFormat>();
            usersRanges = new Dictionary<int, Tuple<int, int>>();
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
                DateTime date = default;
                string text = "";
                int number = 0;
                bool isBackward = false;
                bool isShowThisItem = false;
                var expecting = GetExpecting(userId);
                var expectingFormat = expecting.Item1;
                var expectingRange = Tuple.Create(expecting.Item2, expecting.Item3);
                if (GetStringWithoutOpeningSlash(message).ToLower() == "back")
                {
                    isBackward = true;
                    OnMessageRecieved?.Invoke(userId, date, text, number, isBackward, isShowThisItem);
                    return;
                }
                switch (expectingFormat)
                {
                    case ExpectingRequestFormat.FirstRequest:
                        isShowThisItem = true;
                        break;
                    case ExpectingRequestFormat.Text:
                        text = message;
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

                        date = dateTime;
                        break;
                    }
                    case ExpectingRequestFormat.OnlyDayDateTime:
                    {
                        var isDateTime = DateTime.TryParse(message, out var dateTime);
                        if (!isDateTime)
                        {
                            bot.SendTextMessageAsync(chatId,
                                "Некорректный формат даты.\nПопробуйте ДД-ММ-ГГГГ\n/Back");
                            return;
                        }
                        
                        date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                        break;
                    }
                    case ExpectingRequestFormat.Number:
                        message = GetStringWithoutOpeningSlash(message);
                        var isInt = int.TryParse(message, out var outNumber);
                        if (!isInt || outNumber < expectingRange.Item1 || outNumber > expectingRange.Item2)
                        {
                            bot.SendTextMessageAsync(chatId, "Неверное число\n/Back");
                            return;
                        }

                        number = outNumber;
                        break;
                    case ExpectingRequestFormat.OnlyTimeDateTime:
                    {
                        var isDateTime = DateTime.TryParse(message, out var dateTime);
                        if (!isDateTime)
                        {
                            bot.SendTextMessageAsync(chatId,
                                "Некорректный формат времени.\nПопробуйте ЧЧ:ММ\n/Back");
                            return;
                        }

                        date = dateTime;
                        break;
                    }
                }
                OnMessageRecieved?.Invoke(userId, date, text, number, isBackward, isShowThisItem);
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

        private Tuple<ExpectingRequestFormat, int, int> GetExpecting(int userId)
        {
            if (!usersExpectingFormats.ContainsKey(userId) || !usersRanges.ContainsKey(userId))
            {
                return Tuple.Create(ExpectingRequestFormat.FirstRequest, 0, 0);
            }

            return Tuple.Create(usersExpectingFormats[userId], usersRanges[userId].Item1, usersRanges[userId].Item2);
        }

        public void SendAnswer(int userId, string headline, string[] items, ExpectingRequestFormat format)
        {
            var answerText = new StringBuilder();
            answerText.Append(headline + "\n");
            var i = 1;
            foreach (var answerItem in items)
            {
                answerText.Append("/" + i + " - " + answerItem + "\n");
                i++;
            }

            answerText.Append("/Back");
            usersExpectingFormats[userId] = format;
            usersRanges[userId] = Tuple.Create(1, items.Length);
            bot.SendTextMessageAsync(chatIds[userId], answerText.ToString());
        }
    }
}
