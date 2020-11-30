﻿using System;

namespace Organizer
{
    public class UiRequest
    {
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public bool IsBackward { get; set; }
        public bool IsShowThisItem { get; set; }
    }
    
    public class Request
    {
        public State State { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public Request(State state, DateTime date, string text)
        {
            State = state;
            Date = date;
            Text = text;
        }
    }
}