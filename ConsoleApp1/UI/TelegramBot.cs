using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Organizer
{
    public enum ExpectingRequest
    {
        Text,
        DateTime,
        Number
    }
    public class Answer
    {
        public string Text { get; set; }
        public ExpectingRequest Format { get; set; }
        public bool IsBack { get; set; }
        public Tuple<int, int> NumberRange { get; set; }
    }
    
    public class TelegramBot : IUi
    {
        private Dictionary<long, long> chatIds;
        private TelegramBotClient bot;
        private Dictionary<int, ExpectingRequest> answers;
        public event Action<Request> OnMessageRecieved;

        public TelegramBot()
        {
            chatIds = new Dictionary<long, long>();
            bot = new TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
            bot.OnMessage += ReadMessage;
            bot.StartReceiving();
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
                if (answers.ContainsKey(userId))
                {
                    var lastAnswer = answers[userId];
                }
                OnMessageRecieved!(new Request(new State(userId, 0, 0), default, message));
            }
        }

        public void SendAnswer(Request answer)
        {
            bot.SendTextMessageAsync(chatIds[answer.State.UserId], answer.Text);
        }
    }
}
