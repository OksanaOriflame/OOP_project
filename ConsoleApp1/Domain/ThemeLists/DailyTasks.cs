using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Organizer
{
    public class DailyTasks
    {
        private Dictionary<DateTime, List<TaskItem>> tasks;
        private List<DateTime> dateTimes;

        public DailyTasks(List<Tuple<DateTime, List<string>>> tasksOnDates)
        {
            dateTimes = tasksOnDates
                .Select(t => t.Item1)
                .ToList();
            tasks = new Dictionary<DateTime, List<TaskItem>>(tasksOnDates
                    .Select(t => new KeyValuePair<DateTime, List<TaskItem>>(t.Item1, 
                        t.Item2
                            .Select(taskName => new TaskItem()
                            {
                                DateTime = t.Item1,
                                Text = taskName
                            })
                            .ToList())));
        }

        public void AddNewTask(DateTime date, string name)
        {
            tasks[date].Add(new TaskItem()
            {
                DateTime = date,
                Text = name
            });
            tasks[date] = tasks[date].OrderBy(t => t.Text).ToList();
        }

        public void AddNewDate(DateTime date)
        {
            dateTimes.Add(date);
            dateTimes = dateTimes.OrderBy(d => d).ToList();
            tasks[date] = new List<TaskItem>();
        }

        public void DeleteTask(DateTime date, int taskNumber)
        {
            tasks[date].RemoveAt(taskNumber);
        }

        public void ChangeTaskName(DateTime date, int taskNumber, string name)
        {
            tasks[date][taskNumber] = new TaskItem() {DateTime = date, Text = name};
        }

        public List<string> GetAllTasksOnDate(DateTime date)
        {
            return tasks[date]
                .Select(t => t.Text)
                .ToList();
        }

        public int GetTasksCountOnDate(DateTime date)
        {
            return tasks[date].Count;
        }

        public List<DateTime> GetAllDateTimes() => dateTimes;

        public int GetDateTimesCount() => dateTimes.Count;
    }
}