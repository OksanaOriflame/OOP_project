using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Organizer
{
    public class TelegramBot : IUi
    {
        private Organizer organizer;
        private Thread messageReader;
        private Thread messageWriter;
        private ConcurrentQueue<RequestHandler> requests;
        private ConcurrentQueue<RequestHandler> answers;
        private Dictionary<long, long> chatIds;
        private TelegramBotClient bot;

        public TelegramBot()
        {
            messageReader = new Thread(ReadMessages);
            messageWriter = new Thread(SendMessages);
            requests = new ConcurrentQueue<RequestHandler>();
            answers = new ConcurrentQueue<RequestHandler>();
            chatIds = new Dictionary<long, long>();
            bot = new Telegram.Bot.TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
        }

        public void Start()
        {
            messageReader.Start();
            messageWriter.Start();
        }

        private void SendMessages()
        {
            while (true)
            {
                if (answers.TryDequeue(out var answer))
                {
                    bot.SendTextMessageAsync(chatIds[answer.UserId], answer.Text);
                }
            }
        }

        private void ReadMessages()
        {
            var bot = new Telegram.Bot.TelegramBotClient("1316902632:AAHmu0MM7-QD7j_kwTMnIRkWEcr49In5_mM");
            var offset = 0;
            while (true)
            {
                var updates = bot.GetUpdatesAsync(offset);
                foreach (var update in updates.Result)
                {
                    var userId = update.Message.From.Id;
                    var chatId = update.Message.Chat.Id;
                    chatIds[userId] = chatId;
                    var message = update.Message.Text;
                    if (message == null)
                    {
                        bot.SendTextMessageAsync(update.Message.Chat.Id, "Я не понимаю. Напиши словами, плес");
                    }
                    else
                    {
                        Console.WriteLine(message);
                        requests.Enqueue(new RequestHandler(userId, new State(0, 0), default, message));
                    }
                    offset = update.Id + 1;
                }
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
            answers.Enqueue(answer);
        }
    }
}
