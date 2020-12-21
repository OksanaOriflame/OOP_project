using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Organizer
{
    public class DeadlineTasks
    {
        private List<TaskItem> tasks;
        public DeadlineTasks(Tuple<DateTime, string>[] tasks)
        {
            this.tasks = tasks
                .Select(t => new TaskItem() {DateTime = t.Item1, Text = t.Item2})
                .ToList();
        }

        public List<string> GetAllTasks()
        {
            return tasks
                .Select(t => t.Text + string.Format(" (до {0})", t.DateTime))
                .ToList();
        }

        public List<Tuple<DateTime, string>> GetAllTasksTuples()
        {
            return tasks
                .Select(t => Tuple.Create(t.DateTime, t.Text))
                .ToList();
        }

        public void DeleteTask(int taskNumber)
        {
            tasks.RemoveAt(taskNumber);
        }

        public void AddTask(string name, DateTime deadline)
        {
            tasks.Add(new TaskItem() {DateTime = deadline, Text = name});
            tasks = tasks.OrderBy(t => t.DateTime).ToList();
        }
    }
}
