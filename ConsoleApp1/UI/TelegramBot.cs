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
    public class TelegramBot : IUi
    {
        private ConcurrentQueue<RequestHandler> requests;
        private Dictionary<long, long> chatIds;
        private TelegramBotClient bot;
        public event Action<RequestHandler> OnMessageRecieved;

        public TelegramBot()
        {
            requests = new ConcurrentQueue<RequestHandler>();
            chatIds = new Dictionary<long, long>();
            bot = new TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
            bot.OnMessage += ReadMessages;
            bot.StartReceiving();
        }

        private void ReadMessages(object sender, MessageEventArgs args)
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
                Console.WriteLine(message);
                OnMessageRecieved(new RequestHandler(userId, new State(0, 0), default, message));
            }
        }

        public RequestHandler GetNextRequest()
        {
            if (requests.TryDequeue(out var result))
                return result;
            return null;
        }

        public void SendAnswer(RequestHandler answer)
        {
            bot.SendTextMessageAsync(chatIds[answer.UserId], answer.Text);
        }
    }
}
