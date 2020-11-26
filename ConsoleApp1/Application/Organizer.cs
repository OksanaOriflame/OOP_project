using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Organizer
{
    public class RequestHandler
    {
        public int UserId { get; set; }
        public State State { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public RequestHandler(int userId, State state, DateTime date, string text)
        {
            UserId = userId;
            State = state;
            Date = date;
            Text = text;
        }
    }

    public class State
    {
        public int StateId { get; set; }
        public int SubStateId { get; set; }

        public State(int stateId, int subStateId)
        {
            StateId = stateId;
            SubStateId = subStateId;
        }
    }

    public class Organizer
    {
        private IDataBase dataBase;
        private Dictionary<int, IOrganizerItem> items;
        private IUi ui;
        private Thread worker;

        public Organizer(IOrganizerItem[] items, IDataBase dataBase, IUi ui)
        {
            this.items = new Dictionary<int, IOrganizerItem>(items.Select(item => new KeyValuePair<int, IOrganizerItem>(item.GetId(), item)));
            this.dataBase = dataBase;
            this.ui = ui;
            worker = new Thread(Work);
        }

        public Dictionary<string, int> GetGlobalOptions()
        {
            return new Dictionary<string, int>(items.Select(item => new KeyValuePair<string, int>(item.Value.GetName(), item.Key)));
        }

        public void Start()
        {
            ui.Start();
            worker.Start();
        }

        private void Work()
        {
            var a = 1;
            while (true)
            {
                var request = ui.GetNextRequest();
                if (request != null)
                {
                    ui.SendAnswer(new RequestHandler(request.UserId, default, default, a.ToString()));
                    a++;
                }
            }
        }
    }
}
